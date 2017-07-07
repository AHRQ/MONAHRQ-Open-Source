using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using Monahrq.Infrastructure.Extensions;

namespace Monahrq.Wing.Dynamic.Models
{
    /// <summary>
    /// Dynamic target builder class
    /// </summary>
    public class DynamicTargetBuilder
    {
        /// <summary>
        /// Gets or sets the type of the object.
        /// </summary>
        /// <value>
        /// The type of the object.
        /// </value>
        public Type ObjType { get; set; }
        /// <summary>
        /// Gets the name of the target.
        /// </summary>
        /// <value>
        /// The name of the target.
        /// </value>
        public static string TargetName { get; private set; }
        /// <summary>
        /// Gets the type of the base.
        /// </summary>
        /// <value>
        /// The type of the base.
        /// </value>
        public static Type BaseType { get; private set; }
        /// <summary>
        /// Gets the name of the assembly.
        /// </summary>
        /// <value>
        /// The name of the assembly.
        /// </value>
        public static string AssemblyName { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DynamicTargetBuilder"/> class.
        /// </summary>
        /// <param name="targetName">Name of the target.</param>
        /// <param name="baseType">Type of the base.</param>
        public DynamicTargetBuilder(string targetName, Type baseType)
        {
            this.ObjType = null;
            TargetName = targetName;
            BaseType = baseType;
            AssemblyName = string.Format("{0}.Entities", typeof(WizardContext).Assembly.GetName().Name);
        }

        /// <summary>
        /// Creates the new object.
        /// </summary>
        /// <param name="fields">The list of <see cref="Field"/>'s.</param>
        /// <returns></returns>
        public object CreateNewObject(List<Field> fields)
        {
            this.ObjType = CompileResultType(fields);
            var myObject = Activator.CreateInstance(this.ObjType);

            return myObject;
        }

        /// <summary>
        /// Gets the object list.
        /// </summary>
        /// <returns></returns>
        public IList GetObjectList()
        {
            var listType = typeof(List<>).MakeGenericType(this.ObjType);

            return (IList)Activator.CreateInstance(listType);
        }

        /// <summary>
        /// Gets the compare to method.
        /// </summary>
        /// <param name="genericInstance">The generic instance.</param>
        /// <param name="sortExpression">The sort expression.</param>
        /// <returns></returns>
        public static MethodInfo GetCompareToMethod(object genericInstance, string sortExpression)
        {
            var genericType = genericInstance.GetType();
            var sortExpressionValue = genericType.GetProperty(sortExpression).GetValue(genericInstance, null);
            var sortExpressionType = sortExpressionValue.GetType();
            var compareToMethodOfSortExpressionType = sortExpressionType.GetMethod("CompareTo", new [] { sortExpressionType });

            return compareToMethodOfSortExpressionType;
        }

        /// <summary>
        /// Compiles the type of the result.
        /// </summary>
        /// <param name="fields">The fields.</param>
        /// <returns></returns>
        public static Type CompileResultType(List<Field> fields)
        {
            var tb = GetTypeBuilder();
            var constructor = tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

            // NOTE: assuming your list contains Field objects with fields FieldName(string) and FieldType(Type)
            foreach (var field in fields.ToList())
                CreateProperty(tb, field);

            var objectType = tb.CreateType();
            return objectType;
        }

        /// <summary>
        /// Gets the type builder.
        /// </summary>
        /// <returns></returns>
        private static TypeBuilder GetTypeBuilder()
        {
            var typeSignature = TargetName ?? "DynamicTarget";
            var an = new AssemblyName(AssemblyName);
            var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(an, AssemblyBuilderAccess.Run);
            var moduleBuilder = assemblyBuilder.DefineDynamicModule(AssemblyName);
            var tb = moduleBuilder.DefineType(typeSignature
                                              , TypeAttributes.Public |
                                                TypeAttributes.Class |
                                                TypeAttributes.AutoClass |
                                                TypeAttributes.AnsiClass |
                                                TypeAttributes.BeforeFieldInit |
                                                TypeAttributes.AutoLayout
                                              , BaseType);
            return tb;
        }

        /// <summary>
        /// Creates the property.
        /// </summary>
        /// <param name="tb">The type builder.</param>
        /// <param name="field">The field.</param>
        private static void CreateProperty(TypeBuilder tb, Field field)
        {
            var propertyName = field.FieldName;
            var propertyType = field.FieldType;

            var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            var getPropMthdBldr = tb.DefineMethod("get_" + propertyName, MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
            var getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            var setPropMthdBldr =
                    tb.DefineMethod("set_" + propertyName,
                                    MethodAttributes.Public |
                                    MethodAttributes.SpecialName |
                                    MethodAttributes.HideBySig,
                                    null, new[] { propertyType });

            var setIl = setPropMthdBldr.GetILGenerator();
            var modifyProperty = setIl.DefineLabel();
            var exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);

            GetValidationAttributesFromDataType(propertyBuilder, field);
            
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

        /// <summary>
        /// Gets the type of the validation attributes from data type.
        /// </summary>
        /// <param name="propertyBuilder">The property builder.</param>
        /// <param name="field">The field.</param>
        private static void GetValidationAttributesFromDataType(PropertyBuilder propertyBuilder, Field field)
        {
            var propertyType = field.FieldType.IsGenericType
                ? field.FieldType.GenericTypeArguments.ToList().First()
                : field.FieldType;

            CustomAttributeBuilder attrBuilder;
            if (propertyType.IsEnum)
            {
                var values = Enum.GetValues(propertyType).Cast<int>().ToArray();
                var rangeAtribute = typeof(RangeAttribute);
                var constructorInfo = rangeAtribute.GetConstructors()[0];
                attrBuilder = new CustomAttributeBuilder(constructorInfo, new object[] { values.Min(), values.Max() });
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }

            if (field.IsRequired)
            {
                var requiredAttributeType = typeof(RequiredAttribute);
                attrBuilder = new CustomAttributeBuilder(requiredAttributeType.GetConstructors()[0], requiredAttributeType.GetConstructors()[0].GetParameters().Cast<object>().ToArray());
                propertyBuilder.SetCustomAttribute(attrBuilder);
            }
        }
    }
}