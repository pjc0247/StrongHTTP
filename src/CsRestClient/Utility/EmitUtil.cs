using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Reflection.Emit;

namespace CsRestClient.Utility
{
    static class EmitUtil
    {
        public static MethodBuilder CreateMethod(
            this TypeBuilder typeBuilder,
            string name,
            Type returnType, Type[] paramTypes)
        {
            var methodBuilder = typeBuilder.DefineMethod(
                name,
                MethodAttributes.Public |
                MethodAttributes.Virtual |
                MethodAttributes.NewSlot |
                MethodAttributes.HideBySig |
                MethodAttributes.Final,
                returnType,
                paramTypes);

            return methodBuilder;
        }

        public static TypeBuilder CreateProperty(
            this TypeBuilder typeBuilder,
            string name, Type type,
            bool getter,bool setter)
        {
            var meta = typeBuilder.DefineField(
                    "_" + name, type, FieldAttributes.Private);

            if (getter)
            {
                var methodBuilder = typeBuilder.DefineMethod(
                    "get_" + name,
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.NewSlot |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.Final,
                    type,
                    null);
                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldfld, meta);
                ilGen.Emit(OpCodes.Ret);
            }
            if (setter)
            {
                var methodBuilder = typeBuilder.DefineMethod(
                    "set_" + name,
                    MethodAttributes.Public |
                    MethodAttributes.Virtual |
                    MethodAttributes.NewSlot |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.Final,
                    null,
                    new Type[] { type });
                var ilGen = methodBuilder.GetILGenerator();

                ilGen.Emit(OpCodes.Ldarg_0);
                ilGen.Emit(OpCodes.Ldarg_1);
                ilGen.Emit(OpCodes.Stfld, meta);
                ilGen.Emit(OpCodes.Ret);
            }

            return typeBuilder;
        }


    }
}
