// This file is part of PosiStageDotNet.
// 
// PosiStageDotNet is free software: you can redistribute it and/or modify
// it under the terms of the GNU Lesser General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// PosiStageDotNet is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with PosiStageDotNet.  If not, see <http://www.gnu.org/licenses/>.

using System.Reflection;
using System.Resources;
using System.Runtime.CompilerServices;
[assembly: AssemblyTitle("Imp.PosiStageDotNet")]
[assembly: AssemblyDescription("C# PCL for sending/receiving PosiStageNet data over Ethernet")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else
	[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("The Impersonal Stereo")]
[assembly: AssemblyProduct("Imp.PosiStageDotNet")]
[assembly: AssemblyCopyright("Copyright © David Butler / The Impersonal Stereo 2016")]
[assembly: NeutralResourcesLanguage("en")]
[assembly: InternalsVisibleTo("Imp.PosiStageDotNet.Tests")]