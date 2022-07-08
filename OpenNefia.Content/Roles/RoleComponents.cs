using OpenNefia.Core.GameObjects;
using System;
using System.Collections.Generic;

namespace OpenNefia.Content.Roles
{
    public interface IRoleComponent : IComponent
    {
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>3</hspId>
    [RegisterComponent]
    public class RoleSpecialComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleSpecial";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>4</hspId>
    [RegisterComponent]
    public class RoleCitizenComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleCitizen";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>5</hspId>
    [RegisterComponent]
    public class RoleIdentifierComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleIdentifier";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>6</hspId>
    [RegisterComponent]
    public class RoleElderComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleElder";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>7</hspId>
    [RegisterComponent]
    public class RoleTrainerComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleTrainer";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>8</hspId>
    [RegisterComponent]
    public class RoleInformerComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleInformer";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>9</hspId>
    [RegisterComponent]
    public class RoleBartenderComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleBartender";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>10</hspId>
    [RegisterComponent]
    public class RoleArenaMasterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleArenaMaster";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>11</hspId>
    [RegisterComponent]
    public class RolePetArenaMasterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RolePetArenaMaster";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>12</hspId>
    [RegisterComponent]
    public class RoleHealerComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleHealer";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>13</hspId>
    [RegisterComponent]
    public class RoleAdventurerComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleAdventurer";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>14</hspId>
    [RegisterComponent]
    public class RoleGuardComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleGuard";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>15</hspId>
    [RegisterComponent]
    public class RoleRoyalFamilyComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleRoyalFamily";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>16</hspId>
    [RegisterComponent]
    public class RoleShopGuardComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleShopGuard";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>17</hspId>
    [RegisterComponent]
    public class RoleSlaverComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleSlaver";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>18</hspId>
    [RegisterComponent]
    public class RoleMaidComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleMaid";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>19</hspId>
    [RegisterComponent]
    public class RoleSisterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleSister";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>20</hspId>
    [RegisterComponent]
    public class RoleCustomCharaComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleCustomChara";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>21</hspId>
    [RegisterComponent]
    public class RoleReturnerComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleReturner";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>22</hspId>
    [RegisterComponent]
    public class RoleHorseMasterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleHorseMaster";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>23</hspId>
    [RegisterComponent]
    public class RoleCaravanMasterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleCaravanMaster";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1005</hspId>
    [RegisterComponent]
    public class RoleInnkeeperComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleInnkeeper";
    }

    /// <hspVariant>elona122</hspVariant>
    /// <hspId>1020</hspId>
    [RegisterComponent]
    public class RoleSpellWriterComponent : Component, IRoleComponent
    {
        /// <inheritdoc />
        public override string Name => "RoleSpellWriter";
    }
}
