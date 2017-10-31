// Copyright (c) Alexandre Mutel. All rights reserved.
// Licensed under the BSD-Clause 2 license. 
// See license.txt file in the project root for full license information.

using System;
using System.IO;

namespace Scriban.Syntax
{
    /// <summary>
    /// A script variable
    /// </summary>
    /// <remarks>This class is immutable as all variable object are being shared across all templates</remarks>
    [ScriptSyntax("variable", "<variable_name>")]
    public abstract class ScriptVariable : ScriptVariablePath, IEquatable<ScriptVariable>
    {
        private readonly int _hashCode;

        public static readonly ScriptVariableLocal Arguments = new ScriptVariableLocal(string.Empty);

        public static readonly ScriptVariableLocal BlockDelegate = new ScriptVariableLocal("$");

        public static readonly ScriptVariableLoop LoopFirst = new ScriptVariableLoop("for.first");

        public static readonly ScriptVariableLoop LoopLast = new ScriptVariableLoop("for.last");

        public static readonly ScriptVariableLoop LoopEven = new ScriptVariableLoop("for.even");

        public static readonly ScriptVariableLoop LoopOdd = new ScriptVariableLoop("for.odd");

        public static readonly ScriptVariableLoop LoopIndex = new ScriptVariableLoop("for.index");

        protected ScriptVariable(string name, ScriptVariableScope scope)
        {
            Name = name;
            Scope = scope;
            unchecked
            {
                _hashCode = (Name.GetHashCode() * 397) ^ (int)Scope;
            }
        }

        /// <summary>
        /// Creates a <see cref="ScriptVariable"/> according to the specified name and <see cref="ScriptVariableScope"/>
        /// </summary>
        /// <param name="name">Name of the variable</param>
        /// <param name="scope">Scope of the variable</param>
        /// <returns>The script variable</returns>
        public static ScriptVariable Create(string name, ScriptVariableScope scope)
        {
            switch (scope)
            {
                case ScriptVariableScope.Global:
                    return new ScriptVariableGlobal(name);
                case ScriptVariableScope.Local:
                    return new ScriptVariableLocal(name);
                case ScriptVariableScope.Loop:
                    return new ScriptVariableLoop(name);
                default:
                    throw new InvalidOperationException($"Scope `{scope}` is not supported");
            }
        }

        /// <summary>
        /// Gets or sets the name of the variable (without the $ sign for local variable)
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets or sets a boolean indicating whether this variable is a local variable (starting with $ in the template ) or global.
        /// </summary>
        public ScriptVariableScope Scope { get; }


        public bool Equals(ScriptVariable other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return string.Equals(Name, other.Name) && Scope == other.Scope;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj is ScriptVariable && Equals((ScriptVariable) obj);
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return Scope == ScriptVariableScope.Local ? $"${Name}" : Name;
        }

        public static bool operator ==(ScriptVariable left, ScriptVariable right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(ScriptVariable left, ScriptVariable right)
        {
            return !Equals(left, right);
        }

        public override object Evaluate(TemplateContext context)
        {
            return context.GetValue((ScriptExpression)this);
        }

        public override object GetValue(TemplateContext context)
        {
            return context.GetValue(this);
        }

        public override void SetValue(TemplateContext context, object valueToSet)
        {
            context.SetValue(this, valueToSet);
        }

        public override void Write(RenderContext context)
        {
            context.Write(ToString());
        }
    }

    public sealed class ScriptVariableGlobal : ScriptVariable
    {
        public ScriptVariableGlobal(string name) : base(name, ScriptVariableScope.Global)
        {
        }

        public override object GetValue(TemplateContext context)
        {
            // Used a specialized overrides on contxet for ScriptVariableGlobal
            return context.GetValue(this);
        }
    }


    public sealed class ScriptVariableLocal : ScriptVariable
    {
        public ScriptVariableLocal(string name) : base(name, ScriptVariableScope.Local)
        {
        }
    }

    public sealed class ScriptVariableLoop : ScriptVariable
    {
        public ScriptVariableLoop(string name) : base(name, ScriptVariableScope.Loop)
        {
        }

        public override void Write(RenderContext context)
        {
            if (context.IsInWhileLoop)
            {
                // TODO: Not efficient
                context.Write(ToString().Replace("for", "while"));
            }
            else
            {
                base.Write(context);
            }
        }
    }

}