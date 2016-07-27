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
using System.Runtime.InteropServices;
[assembly: AssemblyTitle("Imp.PosiStageDotNet.Client")]
[assembly: AssemblyDescription("Example to show receiving data using Imp.PosiStageDotNet")]

#if DEBUG

[assembly: AssemblyConfiguration("Debug")]
#else

[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("The Impersonal Stereo")]
[assembly: AssemblyProduct("Imp.PosiStageDotNet")]
[assembly: AssemblyCopyright("Copyright © David Butler / The Impersonal Stereo 2016")]
[assembly: ComVisible(false)]
[assembly: Guid("e08f8047-21bf-43c6-b474-f3f6285efaa7")]