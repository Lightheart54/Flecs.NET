using Flecs.NET.Utilities;
using static Flecs.NET.Bindings.Native;

namespace Flecs.NET.Core
{
    /// <summary>
    ///     Static class for importing modules.
    /// </summary>
    internal static unsafe class Module
    {
        /// <summary>
        ///     Imports a module.
        /// </summary>
        /// <param name="world"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static Entity Import<T>(World world) where T : IFlecsModule, new()
        {
            ulong module = world.LookupSymbol(Type<T>.FullName, true, false);

            if (!Type<T>.IsRegistered(world))
            {
                if (module == 0)
                    module = DoImport<T>(world, Type<T>.FullName);
            }
            else if (module == 0)
            {
                module = DoImport<T>(world, Type<T>.FullName);
            }

            return new Entity(world, module);
        }

        /// <summary>
        ///     Imports a module.
        /// </summary>
        /// <param name="world"></param>
        /// <param name="symbol"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static ulong DoImport<T>(World world, string symbol) where T : IFlecsModule, new()
        {
            ulong scope = ecs_set_scope(world, 0);

            Component<T> moduleComponent = new Component<T>(world);
            ecs_add_id(world, moduleComponent, EcsModule);

            ecs_set_scope(world, moduleComponent);

            T module = new T();
            module.InitModule(world);

            if (Type<T>.Size != 0)
                world.Set(ref module);

            ecs_set_scope(world, scope);

            using NativeString nativeSymbol = (NativeString)symbol;
            ulong moduleEntity = ecs_lookup_symbol(world, nativeSymbol, Macros.False, Macros.False);
            Ecs.Assert(moduleEntity != 0, $"{nameof(ECS_MODULE_UNDEFINED)} {symbol}");
            Ecs.Assert(moduleEntity == moduleComponent, nameof(ECS_INTERNAL_ERROR));

            return moduleEntity;
        }
    }
}
