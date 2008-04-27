//
// Gendarme.Rules.Correctness.MethodCanBeMadeStaticRule
//
// Authors:
//	Jb Evain <jbevain@gmail.com>
//	Sebastien Pouliot <sebastien@ximian.com>
//
// Copyright (C) 2007 Jb Evain
// Copyright (C) 2008 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using Mono.Cecil;
using Mono.Cecil.Cil;

using Gendarme.Framework;
using Gendarme.Framework.Rocks;

namespace Gendarme.Rules.Correctness {

	[Problem ("This method does not use any instance fields, properties or methods and can be made static.")]
	[Solution ("Make this method static.")]
	public class MethodCanBeMadeStaticRule : Rule, IMethodRule {

		public RuleResult CheckMethod (MethodDefinition method)
		{
			// we only check non static, non virtual methods and not constructors
			// we also don't check event callbacks, as they usually bring a lot of false positive,
			// and that developers are tied to their signature
			if (method.IsStatic || method.IsVirtual || method.IsConstructor || method.IsEventCallback ())
				return RuleResult.DoesNotApply;

			// we only check methods with a body
			if (!method.HasBody)
				return RuleResult.DoesNotApply;

			// that aren't compiler generated (e.g. anonymous methods) or generated by a tool (e.g. web services)
			if (method.IsGeneratedCode ())
				return RuleResult.DoesNotApply;

			// rule applies

			// if we find a use of the "this" reference, it's ok
			foreach (Instruction instr in method.Body.Instructions) {
				if (instr.OpCode == OpCodes.Ldarg_0 || (instr.OpCode == OpCodes.Ldarg && (int) instr.Operand == 0))
					return RuleResult.Success;
			}

			Runner.Report (method, Severity.Low, Confidence.Total, string.Empty);
			return RuleResult.Failure;
		}
	}
}
