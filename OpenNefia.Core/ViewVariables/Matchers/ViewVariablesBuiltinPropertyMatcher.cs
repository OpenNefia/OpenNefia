using OpenNefia.Core.Areas;
using OpenNefia.Core.GameObjects;
using OpenNefia.Core.Maps;
using OpenNefia.Core.Maths;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.ViewVariables.Editors;
using static OpenNefia.Core.ViewVariables.Editors.VVPropEditorNumeric;

namespace OpenNefia.Core.ViewVariables
{
    public sealed class ViewVariablesBuiltinPropertyMatcher : ViewVariablesPropertyMatcher
    {
        public override VVPropEditor? PropEditorFor(Type type)
        {
            if (type == typeof(sbyte))
            {
                return new VVPropEditorNumeric(NumberType.SByte);
            }

            if (type == typeof(byte))
            {
                return new VVPropEditorNumeric(NumberType.Byte);
            }

            if (type == typeof(ushort))
            {
                return new VVPropEditorNumeric(NumberType.UShort);
            }

            if (type == typeof(short))
            {
                return new VVPropEditorNumeric(NumberType.Short);
            }

            if (type == typeof(uint))
            {
                return new VVPropEditorNumeric(NumberType.UInt);
            }

            if (type == typeof(int))
            {
                return new VVPropEditorNumeric(NumberType.Int);
            }

            if (type == typeof(ulong))
            {
                return new VVPropEditorNumeric(NumberType.ULong);
            }

            if (type == typeof(long))
            {
                return new VVPropEditorNumeric(NumberType.Long);
            }

            if (type == typeof(float))
            {
                return new VVPropEditorNumeric(NumberType.Float);
            }

            if (type == typeof(double))
            {
                return new VVPropEditorNumeric(NumberType.Double);
            }

            if (type == typeof(decimal))
            {
                return new VVPropEditorNumeric(NumberType.Decimal);
            }

            if (type == typeof(string))
            {
                return new VVPropEditorString();
            }

            if (typeof(IPrototype).IsAssignableFrom(type))
            {
                return (VVPropEditor)Activator.CreateInstance(typeof(VVPropEditorIPrototype<>).MakeGenericType(type))!;
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(PrototypeId<>))
            {
                var prototypeType = type.GetGenericArguments().First();
                return (VVPropEditor)Activator.CreateInstance(typeof(VVPropEditorIPrototypeId<>).MakeGenericType(prototypeType))!;
            }

            if (typeof(ISelfSerialize).IsAssignableFrom(type))
            {
                return (VVPropEditor)Activator.CreateInstance(typeof(VVPropEditorISelfSerializable<>).MakeGenericType(type))!;
            }

            if (type.IsEnum)
            {
                return new VVPropEditorEnum();
            }

            if (type == typeof(Vector2))
            {
                return new VVPropEditorVector2(intVec: false);
            }

            if (type == typeof(Vector2i))
            {
                return new VVPropEditorVector2(intVec: true);
            }

            if (type == typeof(bool))
            {
                return new VVPropEditorBoolean();
            }

            if (type == typeof(Angle))
            {
                return new VVPropEditorAngle();
            }

            if (type == typeof(Box2))
            {
                return new VVPropEditorUIBox2(VVPropEditorUIBox2.BoxType.Box2);
            }

            if (type == typeof(Box2i))
            {
                return new VVPropEditorUIBox2(VVPropEditorUIBox2.BoxType.Box2i);
            }

            if (type == typeof(UIBox2))
            {
                return new VVPropEditorUIBox2(VVPropEditorUIBox2.BoxType.UIBox2);
            }

            if (type == typeof(UIBox2i))
            {
                return new VVPropEditorUIBox2(VVPropEditorUIBox2.BoxType.UIBox2i);
            }

            if (type == typeof(EntityCoordinates))
            {
                return new VVPropEditorEntityCoordinates();
            }

            if (type == typeof(EntityUid))
            {
                return new VVPropEditorEntityUid();
            }

            if (type == typeof(MapId))
            {
                return new VVPropEditorMapId();
            }

            if (type == typeof(AreaId))
            {
                return new VVPropEditorAreaId();
            }

            if (type == typeof(Color))
            {
                return new VVPropEditorColor();
            }

            if (type == typeof(TimeSpan))
            {
                return new VVPropEditorTimeSpan();
            }

            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(KeyValuePair<,>))
            {
                return new VVPropEditorKeyValuePair();
            }

            if (!type.IsValueType)
            {
                return new VVPropEditorReference();
            }

            return null;
        }
    }
}