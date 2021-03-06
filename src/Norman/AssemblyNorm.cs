// Copyright (C) 2011, Krzysztof Kozmic 
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without
// modification, are permitted provided that the following conditions are met:
//     * Redistributions of source code must retain the above copyright
//       notice, this list of conditions and the following disclaimer.
//     * Redistributions in binary form must reproduce the above copyright
//       notice, this list of conditions and the following disclaimer in the
//       documentation and/or other materials provided with the distribution.
//     * Neither the name of the <organization> nor the
//       names of its contributors may be used to endorse or promote products
//       derived from this software without specific prior written permission.
// 
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
// ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
// WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
// DISCLAIMED. IN NO EVENT SHALL <COPYRIGHT HOLDER> BE LIABLE FOR ANY
// DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
// (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
// LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
// ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
// (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
// SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

namespace Norman
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using System.Reflection;

	using Mono.Cecil;

	public class AssemblyNorm : INorm
	{
		private readonly Predicate<AssemblyDefinition> assemblyDiscovery;
		private readonly List<INorm> inner = new List<INorm>();
		private IEnumerable<AssemblyDefinition> matchedAssemblies;

		public AssemblyNorm(Predicate<AssemblyDefinition> assemblyDiscovery)
		{
			this.assemblyDiscovery = assemblyDiscovery;
		}

		public TypeNorm ForTypes(Predicate<TypeDefinition> typeDiscovery)
		{
			var norm = new TypeNorm(this, typeDiscovery);
			inner.Add(norm);
			return norm;
		}

		internal IEnumerable<AssemblyDefinition> GetMatchedAssemblies()
		{
			if (matchedAssemblies == null)
			{
				matchedAssemblies = MatchAssemblies().ToArray();
			}
			return matchedAssemblies;
		}

		private IEnumerable<AssemblyDefinition> MatchAssemblies()
		{
			return GetAssemblyDefinitions().Where(assemblyDiscovery.Invoke);
		}

		private IEnumerable<AssemblyDefinition> GetAssemblyDefinitions()
		{
			foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
			{
				yield return GetAssemblyDefinition(assembly);
			}
		}

		private AssemblyDefinition GetAssemblyDefinition(Assembly assembly)
		{
			return AssemblyDefinition.ReadAssembly(assembly.Location);
		}

		void INorm.Verify(IAssert assert)
		{
			inner.ForEach(i => i.Verify(assert));
		}
	}
}