﻿using Metalama.Aspects;
using Metalama.Framework.Aspects;
using Metalama.Framework.Code;
using Metalama.Framework.Fabrics;

[assembly: AspectOrder(typeof(IntroductionAttribute), typeof(LoggingAspect), typeof(OverrideAttribute), typeof(OverrideEventAttribute) )]

namespace Metalama.Aspects
{
    #region other issues
    // Libraries\Nop.Core\Caching\DistributedCacheManager throws error on VSX preview
    #endregion

    #region fabrics

    public class MethodFabric : TransitiveProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            #region fixed and working
            /* Fixed and working tests */

            // FIXED - Error LAMA0041: 'FabricAspect.OverrideMethod()' threw 'AssertionFailedException' when applied to X
            amender.With(p => p.Types.SelectMany(t => t.Methods).Where(m => m.IsAsync && !m.IsOpenGeneric)).AddAspect<LoggingAspect>();

            // FIXED - Error LAMA0611: Error CS8648 in code generated by Metalama: A goto cannot jump to a location after a using declaration.
            amender.With(p => p.Types.SelectMany(t => t.Methods).Where(m => !m.IsAbstract && !m.IsImplicitlyDeclared)).AddAspect<LoggingAspect>();

            // FIXED - error CS1061: 'TEntity' does not contain a definition for 'Id' and no accessible extension method 'Id' accepting a first argument of type 'TEntity' could be found (are you missing a using directive or an assembly reference?)
            // FIXED - error CS0452: The type 'TEntity' must be a reference type in order to use it as parameter 'T' in the generic type or method 'DataConnection.GetTable<T>()'
            amender.With(p => p.Types.SelectMany(t => t.Methods).Where(m => m.IsAsync && m.IsOpenGeneric)).AddAspect<LoggingAspect>();

            #endregion

            #region errors
            /*
            Error when applied on Introduced methods.
             C:\src\Metalama.Tests.NopCommerce\src\Libraries\Nop.Core\obj\Debug\net6.0\metalama\Caching\CacheKey.cs(148,19):
                error LAMA0611: Error CS0140 in code generated by Metalama: The label '__aspect_return_1' is a duplicate
            */
            //amender
            //    .With(p => p.Types.SelectMany(t => t.Methods).Where(m => !m.IsAbstract && !m.GetIteratorInfo().IsIterator && !m.GetAsyncInfo().IsAwaitable))
            //    .AddAspect<ParameterizedLoggingAspect>();

            #endregion
        }
    }

    public class PropertiesFabric : TransitiveProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            #region fixed and/or working
            /* Fixed and working tests */

            amender.With(p =>
                p.Types.SelectMany(t => t.Properties)
                .Where(it => it is not { IsOverride: true, OverriddenProperty: { IsAbstract: true, GetMethod: not null, SetMethod: null } })
                .Where(it => it.DeclaringType is not { TypeKind: TypeKind.Interface })
                .Where(it => !it.IsAbstract && !it.IsImplicitlyDeclared))
            .AddAspect<OverrideAttribute>();

            // FIXED 
            amender.With(p =>
                p.Types.SelectMany(t => t.Properties)
                .Where(it => it is { IsOverride: true, OverriddenProperty: { IsAbstract: true, GetMethod: not null, SetMethod: null } })
                .Where(it => it.DeclaringType is not { TypeKind: TypeKind.Interface })
                .Where(it => !it.IsAbstract && !it.IsImplicitlyDeclared))
            .AddAspect<OverrideAttribute>();

            // FIXED
            amender.With(p =>
                p.Types.SelectMany(t => t.Properties)
                .Where(it => !it.IsAbstract && !it.IsImplicitlyDeclared)
                .Where(it => it.DeclaringType is { TypeKind: TypeKind.Interface }))
            .AddAspect<OverrideAttribute>();

            #endregion

            #region errors
            /* Tests causing errors */
            #endregion
        }
    }

    public class FieldsFabric : TransitiveProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            #region fixed and/or working
            /* Fixed and working tests */

            // FIXED: CSC : error LAMA0001: Unexpected exception occurred in Metalama: Exception of type 'Metalama.Framework.Engine.AssertionFailedException' was thrown.
            amender.With(p =>
                p.Types.SelectMany(t => t.Fields)
                .Where(it => !it.IsAbstract && !it.IsImplicitlyDeclared)
                .Where(it => it is not IField { Writeability: Writeability.None })
                .Where(it => it.DeclaringType is not { TypeKind: TypeKind.Enum or TypeKind.Interface }))
                .AddAspect<OverrideAttribute>();

            #endregion

            #region errors
            #endregion
        }
    }

    public class EventsFabric : TransitiveProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            // Error: Doesn't do anything after introducing events to some classes.
            amender.With(p => p.Types.SelectMany(t => t.Events).Where(e => !e.IsImplicitlyDeclared)).AddAspect<OverrideEventAttribute>();
        }
    }

    public class InterfaceFabric : TransitiveProjectFabric
    {
        public override void AmendProject(IProjectAmender amender)
        {
            #region fixed and/or working
            /* Fixed and working tests */


            // FIXED - CSC : error LAMA0001: Unexpected exception occurred in Metalama: Exception of type 'Metalama.Framework.Engine.AssertionFailedException' was thrown.
            amender.With(p =>
                p.Types
                .Where(t => !t.IsStatic)
                .Where(t => t is not { TypeKind: TypeKind.Enum or TypeKind.Interface or TypeKind.Delegate })
                .Where(t => t.Name != "Program")) // This suppresses global statements.
            .AddAspect<InterfaceIntroductionAttribute>();

            #endregion

            #region errors
            /* Tests causing errors */

            // CSC : error LAMA0001: Unexpected exception occurred in Metalama: Exception of type 'Metalama.Framework.Engine.AssertionFailedException' was thrown.
            //amender.With(p => 
            //    p.Types
            //    .Where(t => !t.IsStatic)
            //    .Where(t => t is not { TypeKind: TypeKind.Enum or TypeKind.Interface or TypeKind.Delegate } ) 
            //    .Where(t => t.Name == "Program" )) // This suppresses global statements.
            //.AddAspect<InterfaceIntroductionAttribute>();

            #endregion
        }
    }

    #endregion

    #region introduction aspects
    public class IntroductionAttribute : TypeAspect
    {
        [Introduce]
        public int IntroducedProperty;

        [Introduce]
        public static int IntroducedProperty_Static;

        [Introduce]
        public int IntroducedField_Initializer = 42;

        [Introduce]
        public static int IntroducedField_Static_Initializer = 42;

        [Introduce]
        public event EventHandler? IntroducedEvent
        {
            add
            {
                Console.WriteLine("Introduced event add accessor.");
            }

            remove
            {
                Console.WriteLine("Introduced event remove accessor.");
            }
        }
    }

    public class MethodIntroductionAttribute : TypeAspect
    {
        [Introduce]
        public void IntroducedMethod_Void()
        {
            Console.WriteLine("This is introduced method.");
            meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Int()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        [Introduce]
        public int IntroducedMethod_Param(int x)
        {
            Console.WriteLine($"This is introduced method, x = {x}.");

            return meta.Proceed();
        }

        [Introduce]
        public static int IntroducedMethod_StaticSignature()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }

        [Introduce(IsVirtual = true)]
        public int IntroducedMethod_VirtualExplicit()
        {
            Console.WriteLine("This is introduced method.");

            return meta.Proceed();
        }
    }

    public class OverrideEventAttribute : OverrideEventAspect
    {
        public override void OverrideAdd(dynamic value)
        {
            Console.WriteLine("Overriden add.");
            meta.Proceed();
        }

        public override void OverrideRemove(dynamic value)
        {
            Console.WriteLine("Overriden remove.");
            meta.Proceed();
        }
    }

    public interface IIntroducedInterface
    {
        int InterfaceMethod();

        event EventHandler InterfaceEvent;

        event EventHandler? InterfaceEventField;

        int Property { get; set; }

        string AutoProperty { get; set; }
    }

    public class InterfaceIntroductionAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            builder.Advice.ImplementInterface(builder.Target, typeof(IIntroducedInterface), whenExists: OverrideStrategy.Ignore );
        }

        [InterfaceMember(IsExplicit = true)]
        public int InterfaceMethod()
        {
            Console.WriteLine("This is introduced interface member.");
            return meta.Proceed();
        }

        [InterfaceMember(IsExplicit = true)]
        public event EventHandler? InterfaceEvent
        {
            add
            {
                Console.WriteLine("This is introduced interface member.");
                meta.Proceed();
            }

            remove
            {
                Console.WriteLine("This is introduced interface member.");
                meta.Proceed();
            }
        }

        [InterfaceMember(IsExplicit = true)]
        public event EventHandler? InterfaceEventField = default;

        [InterfaceMember(IsExplicit = true)]
        public int Property
        {
            get
            {
                Console.WriteLine("This is introduced interface member.");

                return meta.Proceed();
            }

            set
            {
                Console.WriteLine("This is introduced interface member.");
                meta.Proceed();
            }
        }

        [InterfaceMember(IsExplicit = true)]
        public string? AutoProperty { get; set; } = default;
    }

    #endregion

    #region override aspects

    public class LoggingAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            Console.WriteLine($"Executing {meta.Target.Method.ToDisplayString()}");

            return meta.Proceed();

        }
    }

    public class ParameterizedLoggingAspect : OverrideMethodAspect
    {
        public override dynamic? OverrideMethod()
        {
            try
            {
                var result = meta.Proceed();

                Console.WriteLine($"Executing {meta.Target.Method.ToDisplayString()}");
                var parameters = meta.Target.Parameters;

                if (parameters.Count > 0)
                {
                    foreach (var parameter in parameters)
                    {
                        Console.WriteLine($"Method has parameter {parameter.Name} of type {parameter.Type} with {parameter.DefaultValue} default value.");
                    }
                    return result;
                }
                else
                {
                    Console.WriteLine("Parameterless method.");
                    return result;
                }
            }
            catch ( Exception e )
            {
                Console.WriteLine($"Caught exception {e.Message} in {meta.Target.Method.Name}");
                throw;
            }
        }
    }

    public class OverrideAttribute : OverrideFieldOrPropertyAspect
    {
        public override dynamic? OverrideProperty
        {
            get
            {
                Console.WriteLine("This is the overridden getter.");
                return meta.Proceed();
            }

            set
            {
                Console.WriteLine($"This is the overridden setter.");
                meta.Proceed();
            }
        }
    }

    public class OverrideFinalizerAttribute : TypeAspect
    {
        public override void BuildAspect(IAspectBuilder<INamedType> builder)
        {
            var introductionResult = builder.Advice.IntroduceFinalizer(builder.Target, nameof(IntroduceTemplate), whenExists: OverrideStrategy.Override);
        }

        [Template]
        public dynamic? IntroduceTemplate()
        {
            Console.WriteLine("This is the introduction.");
            return meta.Proceed();
        }
    }

    #endregion
}
