using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TypeWebSocket
{
    // RPC 分发器
    public static class RpcDispatcher
    {
        private static readonly Dictionary<Type, object> _services = new();

        // 注册服务
        public static void RegisterService<TInterface, TImplementation>()
            where TImplementation : class, TInterface, new()
        {
            _services[typeof(TInterface)] = new TImplementation();
        }

        public static void RegisterService<TInterface>(TInterface implementation)
            where TInterface : class
        {
            _services[typeof(TInterface)] = implementation ?? throw new ArgumentNullException(nameof(implementation));
        }

        // 自动注册服务（通过反射扫描）
        public static void RegisterServicesFromAssembly(Assembly assembly)
        {
            var serviceTypes = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && t.GetCustomAttribute<RpcServiceAttribute>() != null);

            foreach (var type in serviceTypes)
            {
                var iface = type.GetInterfaces().FirstOrDefault();
                if (iface != null)
                {
                    _services[iface] = Activator.CreateInstance(type)
                        ?? throw new InvalidOperationException($"无法创建 {type.FullName} 的实例。");
                }
            }
        }

        public static async Task<object?> DispatchAsync(byte[] buffer)
        {
            var jsonInput = Encoding.UTF8.GetString(buffer, 0, buffer.Length);
            return await DispatchAsync(jsonInput);
        }

        // 异步分发方法
        public static async Task<object?> DispatchAsync(string jsonInput)
        {
            try
            {
                // 解析 JSON 输入
                var request = JsonSerializer.Deserialize<RpcRequest>(jsonInput);
                if (request == null || string.IsNullOrEmpty(request.Interface) || string.IsNullOrEmpty(request.Method))
                    throw new InvalidOperationException("无效的 JSON 输入。");

                // 查找接口类型
                var iface = _services.Keys.FirstOrDefault(t =>
                    t.Name == request.Interface || t.FullName?.EndsWith(request.Interface) == true);
                if (iface == null)
                    throw new InvalidOperationException($"接口 '{request.Interface}' 未注册。");

                // 获取服务实例
                var instance = _services[iface];
                var method = iface.GetMethod(request.Method);
                if (method == null)
                    throw new InvalidOperationException($"接口 '{request.Interface}' 中未找到方法 '{request.Method}'。");

                // 验证参数数量
                var parameters = method.GetParameters();
                if (request.Args.Length != parameters.Length)
                    throw new InvalidOperationException($"参数数量不匹配：需要 {parameters.Length} 个，实际提供 {request.Args.Length} 个。");

                // 处理参数
                object?[] args = request.Args.Select((arg, i) =>
                {
                    var paramType = parameters[i].ParameterType;
                    if (arg == null)
                        return null;

                    // 如果参数已经是目标类型，直接使用
                    if (paramType.IsInstanceOfType(arg))
                        return arg;

                    // 否则尝试将 arg 序列化为 JSON 再反序列化为目标类型
                    try
                    {
                        var json = JsonSerializer.Serialize(arg);
                        return JsonSerializer.Deserialize(json, paramType);
                    }
                    catch (Exception ex)
                    {
                        throw new InvalidOperationException($"参数 {i} 反序列化失败：{ex.Message}");
                    }
                }).ToArray();

                // 执行方法调用
                if (method.ReturnType.IsGenericType && method.ReturnType.GetGenericTypeDefinition() == typeof(Task<>))
                {
                    var task = (Task?)method.Invoke(instance, args);
                    if (task == null)
                        throw new InvalidOperationException("异步方法调用失败。");
                    await task.ConfigureAwait(false);
                    var resultProperty = task.GetType().GetProperty("Result");
                    return resultProperty?.GetValue(task);
                }
                else
                {
                    return method.Invoke(instance, args);
                }
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"RPC 调用失败: {ex.Message}", ex);
            }
        }
        // 缓存表达式求值器：每个参数表达式编译一次后复用
        private static readonly ConcurrentDictionary<Expression, Func<object?>> _lambdaCache = new();

        private static object? Evaluate(Expression expr)
        {
            if (!_lambdaCache.TryGetValue(expr, out var func))
            {
                func = Expression.Lambda<Func<object?>>(
                    Expression.Convert(expr, typeof(object))
                ).Compile();

                _lambdaCache[expr] = func;
            }
            return func();
        }

        public static async Task SendTextAsync<T>(Expression<Action<T>> expression, Func<byte[], Task> imp)
        {
            if (expression.Body is not MethodCallExpression methodCall)
                throw new ArgumentException("表达式必须是方法调用形式，例如: x => x.Method(...)");

            // 1. 获取接口名和方法名
            var interfaceName = typeof(T).Name;
            var methodName = methodCall.Method.Name;

            // 2. 解析参数值（缓存求值）
            object?[] args = new object?[methodCall.Arguments.Count];
            for (int i = 0; i < methodCall.Arguments.Count; i++)
            {
                var argExpr = methodCall.Arguments[i];
                args[i] = Evaluate(argExpr);
            }

            // 3. 组织消息对象
            var payload = new RpcRequest
            {
                Interface = interfaceName,
                Method = methodName,
                Args = args
            };

            // 4. 转成 JSON
            var json = JsonSerializer.Serialize(payload);

            // 5. 发出去
            var buffer = Encoding.UTF8.GetBytes(json);
            await imp.Invoke(buffer);
        }

    }
}