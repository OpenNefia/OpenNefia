using OpenNefia.Core.Maths;
using OpenNefia.Core.Serialization;
using OpenNefia.Core.Serialization.Manager.Attributes;
using OpenNefia.Core.Utility;
using OpenNefia.VisualAI.UserInterface;
using System.Diagnostics.CodeAnalysis;

namespace OpenNefia.VisualAI.Engine
{
    public sealed record VisualAIPlanError(string Message, Vector2i Position);

    [DataDefinition]
    public sealed class VisualAIPlan : ISerializationHooks
    {
        /// <summary>
        /// Blocks for this plan, not including any inside condition branches.
        /// </summary>
        [DataField("blocks")]
        private List<VisualAIBlock> _blocks { get; } = new();
        public IReadOnlyList<VisualAIBlock> Blocks => _blocks;

        /// <summary>
        /// Branch for the <c>true</c> part of the condition.
        /// Only valid if the last block type is <see cref="VisualAIBlockType.Condition"/>.
        /// </summary>
        [DataField]
        public VisualAIPlan? SubplanTrueBranch { get; set; }

        /// <summary>
        /// Branch for the <c>false</c> part of the condition.
        /// Only valid if the last block type is <see cref="VisualAIBlockType.Condition"/>.
        /// </summary>
        [DataField]
        public VisualAIPlan? SubplanFalseBranch { get; set; }

        /// <summary>
        /// Parent of this plan.
        /// Only valid if the parent ends with a <see cref="VisualAIBlockType.Condition"/> block.
        /// </summary>
        public VisualAIPlan? Parent { get; internal set; }

        public void AfterDeserialization()
        {
            if (SubplanTrueBranch != null)
                SubplanTrueBranch.Parent = this;
            if (SubplanFalseBranch != null)
                SubplanFalseBranch.Parent = this;
        }

        public enum BranchTarget
        {
            TrueBranch,
            FalseBranch
        }

        /// <summary>
        /// The last block in the blocks list to add new blocks after.
        /// </summary>
        public VisualAIBlock? CurrentBlock => Blocks.LastOrDefault();

        /// <summary>
        /// Tests if new blocks can be added.
        /// False if a branch is needed (condition block is last) or the
        /// last block is a terminal action/special block.
        /// </summary>
        public bool CanAddBlock([NotNullWhen(false)] out string? error, VisualAIBlock? block = null)
        {
            var cur = CurrentBlock;
            if (cur == null)
            {
                error = null;
                return true;
            }

            if (cur.Proto.IsTerminal)
            {
                error = $"Plan ends in a terminal action ({cur.ProtoID}).";
                return false;
            }

            if (cur.Proto.Type == VisualAIBlockType.Condition)
            {
                // Blocks should be added onto the true/false subplans instead.
                error = $"Plan ends in a condition ({cur.ProtoID}).";
                return false;
            }

            if (block != null && Blocks.Contains(block))
            {
                error = $"Block {block} already exists in this plan.";
                return false;
            }

            error = null;
            return true;
        }

        public Vector2i GridSize()
        {
            var size = new Vector2i(Blocks.Count, 1);

            if (SubplanTrueBranch != null)
            {
                var innerSize = SubplanTrueBranch.GridSize();
                size = (size.X + innerSize.X, innerSize.Y);
            }

            if (SubplanFalseBranch != null)
            {
                var innerSize = SubplanFalseBranch.GridSize();
                size = (int.Max(size.X, innerSize.X), size.Y + innerSize.Y);
            }

            return size;
        }

        public IEnumerable<VisualAITile> EnumerateTiles() => EnumerateTiles(Vector2i.Zero);
        public IEnumerable<VisualAITile> EnumerateTiles(Vector2i pos)
        {
            for (var i = 0; i < Blocks.Count; i++)
            {
                yield return new VisualAITile.Block(this, pos, Blocks[i]);
                if (i < Blocks.Count - 1)
                    yield return new VisualAITile.Line(this, pos, VisualAITile.LineType.Right, pos + (1, 0));

                pos += (1, 0);
            }

            var block = CurrentBlock;

            if (block == null)
            {
                // Empty plan
                yield return new VisualAITile.Empty(this, pos);
            }
            else if (block.Proto.Type == VisualAIBlockType.Condition)
            {
                DebugTools.AssertNotNull(SubplanTrueBranch);
                DebugTools.AssertNotNull(SubplanFalseBranch);

                yield return new VisualAITile.Line(SubplanTrueBranch, pos + (-1, 0), VisualAITile.LineType.Right, pos);

                foreach (var tile in SubplanTrueBranch.EnumerateTiles(pos))
                    yield return tile;

                var innerSize = SubplanTrueBranch.GridSize();
                var pos2 = pos + (-1, innerSize.Y);
                yield return new VisualAITile.Line(SubplanFalseBranch, pos + (-1, 0), VisualAITile.LineType.Down, pos2);

                foreach (var tile in SubplanFalseBranch.EnumerateTiles(pos2))
                    yield return tile;
            }
            else if (!block.Proto.IsTerminal)
            {
                yield return new VisualAITile.Line(this, pos + (-1, 0), VisualAITile.LineType.Right, pos);
                yield return new VisualAITile.Empty(this, pos);
            }
        }

        public bool Validate(out List<VisualAIPlanError> errors)
            => IsValid(Vector2i.Zero, out errors);

        public bool IsValid(Vector2i pos, out List<VisualAIPlanError> errors)
        {
            errors = new();

            pos += (Blocks.Count, 0);

            var lastBlock = CurrentBlock;

            if (lastBlock == null)
            {
                errors.Add(new("Branch is missing a final block.", pos));
            }
            else if (lastBlock.Proto.Type == VisualAIBlockType.Condition)
            {
                DebugTools.AssertNotNull(SubplanTrueBranch);
                DebugTools.AssertNotNull(SubplanFalseBranch);

                if (!SubplanTrueBranch.IsValid(pos, out var innerErrors))
                    errors.AddRange(innerErrors);

                var innerSize = SubplanTrueBranch.GridSize();
                if (!SubplanFalseBranch.IsValid(pos + (-1, innerSize.Y), out innerErrors))
                    errors.AddRange(innerErrors);
            }
            else if (lastBlock.Proto.Type == VisualAIBlockType.Target)
            {
                errors.Add(new("Target block is missing next block.", pos + (-1, 0)));
            }
            else if (!lastBlock.Proto.IsTerminal)
            {
                errors.Add(new("Non-terminal action is missing next block.", pos + (-1, 0)));
            }

            return errors.Count == 0;
        }

        public bool AddBlock(VisualAIBlock block, [NotNullWhen(false)] out string? error)
        {
            if (!CanAddBlock(out error, block))
                return false;

            DebugTools.AssertNull(SubplanTrueBranch);
            DebugTools.AssertNull(SubplanFalseBranch);

            error = null;
            _blocks.Add(block);

            if (block.Proto.Type == VisualAIBlockType.Condition)
            {
                SubplanTrueBranch = new VisualAIPlan();
                SubplanFalseBranch = new VisualAIPlan();

                SubplanTrueBranch.Parent = this;
                SubplanFalseBranch.Parent = this;
            }

            return true;
        }

        public bool ReplaceBlock(VisualAIBlock block, VisualAIBlock newBlock, [NotNullWhen(false)] out string? error)
        {
            var idx = _blocks.IndexOf(block);
            if (idx == -1)
            {
                error = $"No block {block} exists in this plan.";
                return false;
            }

            void SnipRest()
            {
                _blocks.RemoveRange(idx, _blocks.Count - idx);
            }

            var current = CurrentBlock;
            if (block == current
                && current.Proto.Type == VisualAIBlockType.Condition
                && newBlock.Proto.Type != VisualAIBlockType.Condition)
            {
                SubplanTrueBranch = null;
                SubplanFalseBranch = null;
            }

            if (newBlock.Proto.Type == VisualAIBlockType.Condition)
            {
                if (block.Proto.Type == VisualAIBlockType.Condition)
                {
                    _blocks[idx] = newBlock;
                }
                else
                {
                    SnipRest();
                    return AddBlock(newBlock, out error);
                }
            }
            else if (newBlock.Proto.Type == VisualAIBlockType.Target)
            {
                _blocks[idx] = newBlock;
            }
            else
            {
                if (newBlock.Proto.IsTerminal)
                {
                    SnipRest();
                    return AddBlock(newBlock, out error);
                }
                else
                {
                    _blocks[idx] = newBlock;
                }
            }

            error = null;
            return true;
        }

        public void Merge(VisualAIPlan other)
        {
            if (!CanAddBlock(out var error))
                throw new InvalidOperationException(error);

            SubplanTrueBranch = null;
            SubplanFalseBranch = null;

            if (other.SubplanTrueBranch != null)
            {
                other.SubplanTrueBranch.Parent = this;
                SubplanTrueBranch = other.SubplanTrueBranch;
            }
            if (other.SubplanFalseBranch != null)
            {
                other.SubplanFalseBranch.Parent = this;
                SubplanFalseBranch = other.SubplanFalseBranch;
            }

            _blocks.AddRange(other.Blocks);
        }

        public VisualAIPlan Split(int index)
        {
            index = int.Clamp(index, 0, _blocks.Count - 1);

            var right = new VisualAIPlan();
            bool removeSubplans = false;

            for (var i = index; i < Blocks.Count; i++)
            {
                var block = Blocks[i];

                AddBlock(block, out _);

                if (block.Proto.Type == VisualAIBlockType.Condition)
                {
                    DebugTools.Assert(i == Blocks.Count - 1);
                    right.SubplanTrueBranch = SubplanTrueBranch;
                    right.SubplanFalseBranch = SubplanFalseBranch;
                    removeSubplans = true;
                }
            }

            _blocks.RemoveRange(index, _blocks.Count - index);

            if (removeSubplans)
            {
                SubplanTrueBranch = null;
                SubplanFalseBranch = null;
            }

            return right;
        }

        public bool InsertBlockBefore(VisualAIBlock targetBlock, VisualAIBlock newBlock, [NotNullWhen(false)] out string? error, BranchTarget splitType = BranchTarget.TrueBranch)
        {
            var idx = _blocks.IndexOf(targetBlock);
            if (idx == -1)
            {
                error = $"No block {targetBlock} exists in this plan.";
                return false;
            }

            void SnipRest()
            {
                _blocks.RemoveRange(idx, _blocks.Count - idx);
            }

            if (newBlock.Proto.Type == VisualAIBlockType.Condition)
            {
                var right = Split(idx);
                if (!AddBlock(newBlock, out error))
                    return false;

                switch (splitType)
                {
                    case BranchTarget.TrueBranch:
                    default:
                        SubplanTrueBranch = right;
                        SubplanFalseBranch = new VisualAIPlan();
                        break;
                    case BranchTarget.FalseBranch:
                        SubplanTrueBranch = new VisualAIPlan();
                        SubplanFalseBranch = right;
                        break;
                }
            }
            else
            {
                if (newBlock.Proto.IsTerminal)
                {
                    SnipRest();
                    return AddBlock(newBlock, out error);
                }
                else
                {
                    _blocks.Insert(idx, newBlock);
                }
            }

            error = null;
            return true;
        }

        public void RemoveBlock(VisualAIBlock block, BranchTarget mergeType)
        {
            var idx = _blocks.IndexOf(block);
            if (idx == -1)
            {
                throw new InvalidOperationException($"No block {block} exists in this plan.");
            }

            VisualAIPlan? toMerge = null;

            if (block.Proto.Type == VisualAIBlockType.Condition)
            {
                DebugTools.Assert(idx == Blocks.Count - 1, "Condition block must be at the end of the graph");

                switch (mergeType)
                {
                    case BranchTarget.TrueBranch:
                    default:
                        toMerge = SubplanTrueBranch;
                        break;
                    case BranchTarget.FalseBranch:
                        toMerge = SubplanFalseBranch;
                        break;
                }
            }

            SubplanTrueBranch = null;
            SubplanFalseBranch = null;

            _blocks.RemoveAt(idx);

            if (toMerge != null)
            {
                Merge(toMerge);
            }
        }

        public void RemoveBlockAndRest(VisualAIBlock block)
        {
            var idx = _blocks.IndexOf(block);
            if (idx == -1)
                throw new InvalidOperationException($"No block {block} exists in this plan.");

            SubplanTrueBranch = null;
            SubplanFalseBranch = null;

            _blocks.RemoveAt(idx);
        }

        public void SwapBranches(VisualAIBlock block)
        {
            var idx = _blocks.IndexOf(block);
            if (idx == -1)
                throw new InvalidOperationException($"No block {block} exists in this plan.");

            if (block.Proto.Type != VisualAIBlockType.Condition)
                return;

            var temp = SubplanTrueBranch;
            SubplanTrueBranch = SubplanFalseBranch;
            SubplanFalseBranch = temp;
        }
    }
}