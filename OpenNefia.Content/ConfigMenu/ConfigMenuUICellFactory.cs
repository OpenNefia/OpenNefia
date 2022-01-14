using OpenNefia.Content.ConfigMenu.UICell;
using OpenNefia.Core.IoC;
using OpenNefia.Core.Prototypes;
using OpenNefia.Core.Reflection;
using OpenNefia.Core.UI;
using OpenNefia.Core.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenNefia.Content.ConfigMenu
{
    /// <summary>
    /// Handles instantiating vanilla's config menu UI elements from a set
    /// of config menu node definitions specified in YAML.
    /// </summary>
    public interface IConfigMenuUICellFactory
    {
        void Initialize();

        BaseConfigMenuUICell CreateUICellFor(PrototypeId<ConfigMenuItemPrototype> protoId);
        BaseConfigMenuUICell CreateUICellFor(PrototypeId<ConfigMenuItemPrototype> protoId, Type menuNodeType, IConfigMenuNode menuNode);
    }

    public sealed class ConfigMenuUICellFactory : IConfigMenuUICellFactory
    {
        [Dependency] private readonly IReflectionManager _reflection = default!;
        [Dependency] private readonly IPrototypeManager _protos = default!;

        private readonly Dictionary<Type, Type> _menuNodeTypeToUICellType = new();

        /// <summary>
        /// Given a subclass of <see cref="BaseConfigMenuUICell{Type}"/>, returns
        /// the generic type definition of <see cref="BaseConfigMenuUICell{Type}"/> for that type.
        /// 
        /// e.g. for <see cref="ConfigItemBoolUICell"/>, it should return
        /// <see cref="BaseConfigMenuUICell{ConfigBoolMenuNode}"/>.
        /// </summary>
        private (Type? baseType, Type derivedType) GetMenuNodeTypeForUICellType(Type uiCellDerivedType)
        {
            var uiCellBaseType = TypeHelpers.GetClassHierarchy(uiCellDerivedType)
                .Where(subtype => subtype.IsGenericType && subtype.GetGenericTypeDefinition() == typeof(BaseConfigMenuUICell<>))
                .FirstOrDefault();

            return (uiCellBaseType, uiCellDerivedType);
        }

        public void Initialize()
        {
            var uiCellDerivedTypes = _reflection.GetAllChildren(typeof(BaseConfigMenuUICell))
                .Where(ty => !ty.IsAbstract)
                .Select(GetMenuNodeTypeForUICellType)
                .Where(t => t.baseType != null);

            foreach (var (uiCellBaseType, uiCellActualType) in uiCellDerivedTypes)
            {
                // uiCellBaseType is a generic type of BaseConfigMenuUICell<>.
                // menuNodeType is the type of the config item node coming from YAML.
                var menuNodeType = uiCellBaseType.GetGenericArguments()[0];

                // When the factory finds this menu node type in YAML, instantiate the
                // corresponding UI cell that can accept it as an argument.
                if (_menuNodeTypeToUICellType.TryGetValue(menuNodeType, out var existingUiCellType))
                    throw new InvalidOperationException($"Config menu node type {menuNodeType} already has a UI cell implementation: {existingUiCellType}");

                _menuNodeTypeToUICellType.Add(menuNodeType, uiCellActualType);
            }
        }

        private Type? GetUICellTypeFor(Type menuNodeType)
        {
            return _menuNodeTypeToUICellType.GetValueOrDefault(menuNodeType);
        }

        public BaseConfigMenuUICell CreateUICellFor(PrototypeId<ConfigMenuItemPrototype> protoId, Type menuNodeType, IConfigMenuNode menuNode)
        {
            var uiCellType = GetUICellTypeFor(menuNodeType);
            if (uiCellType == null)
            {
                throw new ArgumentException($"Could not find a subclass of {typeof(BaseConfigMenuUICell)} that accepts {menuNodeType} as an argument.", nameof(menuNode));
            }

            var cell = (BaseConfigMenuUICell)Activator.CreateInstance(uiCellType, new object[] { protoId, menuNode })!;
            IoCManager.InjectDependencies(cell);
            cell.Initialize();
            return cell;
        }

        public BaseConfigMenuUICell CreateUICellFor(PrototypeId<ConfigMenuItemPrototype> protoId)
        {
            var proto = _protos.Index(protoId);
            return CreateUICellFor(protoId, proto.Node.GetType(), proto.Node);
        }
    }
}
