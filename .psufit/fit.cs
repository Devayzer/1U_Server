using System;
using System.Linq;
using System.IO;
using SolidWorks.Interop.sldworks;
class Fit{
static string V(object x){return string.Join(" ",((double[])x).Select(v=>(v*1000).ToString("0.###")));}
[STAThread]static void Main(){var sw=(SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));try{Run(sw);}finally{sw.ExitApp();}}
static void Run(SldWorks sw){int e=0,w=0;var doc=sw.OpenDoc6(Path.GetFullPath("1U Server Assembly.SLDASM"),2,1,"",ref e,ref w);var ass=(AssemblyDoc)doc;var cs=((object[])ass.GetComponents(true)).Cast<Component2>().ToArray();var psu=cs.First(c=>c.Name2.Contains("Flex"));var chassis=cs.First(c=>c.Name2.Contains("Chassis"));Console.WriteLine(V(psu.Transform2.ArrayData));var part=(PartDoc)psu.GetModelDoc2();foreach(Body2 b in (object[])part.GetBodies2(0,false)){Console.WriteLine(b.Name+" "+V(b.GetBodyBox()));foreach(Face2 f in (object[])b.GetFaces()){var s=(Surface)f.GetSurface();var box=(double[])f.GetBox();if(s.IsCylinder()){var c=(double[])s.CylinderParams;if(Math.Abs(c[3])>.99&&box[0]<.001&&c[6]<.0042)Console.WriteLine("hole "+V(c)+" box "+V(box));}}}
}}
