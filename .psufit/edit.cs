using System;
using System.Linq;
using System.IO;
using SolidWorks.Interop.sldworks;
class Fit{
static string V(object x){return string.Join(" ",((double[])x).Select(v=>(v*1000).ToString("0.###")));}
[STAThread]static void Main(){var sw=(SldWorks)Activator.CreateInstance(Type.GetTypeFromProgID("SldWorks.Application"));try{Run(sw);}finally{sw.ExitApp();}}
static void Run(SldWorks sw){int e=0,w=0;var doc=sw.OpenDoc6(Path.GetFullPath("1U Server Assembly.SLDASM"),2,1,"",ref e,ref w);var ass=(AssemblyDoc)doc;var cs=((object[])ass.GetComponents(true)).Cast<Component2>().ToArray();var psu=cs.First(c=>c.Name2.Contains("Flex"));var chassis=cs.First(c=>c.Name2.Contains("Chassis"));var d=(ModelDoc2)chassis.GetModelDoc2();if(d.IsOpenedReadOnly()||doc.IsOpenedReadOnly())throw new Exception("locked");sw.ActivateDoc3(d.GetTitle(),false,0,ref e);
var cut=(Feature)((PartDoc)d).FeatureByName("Rear panel openings");
var sketchFeat=(Feature)cut.GetFirstSubFeature();var sk=(Sketch)sketchFeat.GetSpecificFeature2();var segs=((object[])sk.GetSketchSegments()).Cast<SketchSegment>();var edges=new System.Collections.Generic.List<double[]>();var circles=new System.Collections.Generic.List<double[]>();foreach(var s in segs){if(s.GetType()==0){var line=(SketchLine)s;var a=(SketchPoint)line.GetStartPoint2();var b=(SketchPoint)line.GetEndPoint2();if(a.X<0)edges.Add(new[]{a.X,a.Y,b.X,b.Y});}else{var arc=(SketchArc)s;var p=(SketchPoint)arc.GetCenterPoint2();circles.Add(new[]{p.X,p.Y,arc.GetRadius()});}}
if(edges.Count!=20||circles.Count!=2)throw new Exception("port contours");var oldsk=sketchFeat.Name;d.ClearSelection2(true);cut.Select2(false,0);if(!d.Extension.DeleteSelection2(1))throw new Exception("delete cut");var orphan=(Feature)((PartDoc)d).FeatureByName(oldsk);if(orphan!=null){d.ClearSelection2(true);orphan.Select2(false,0);if(!d.Extension.DeleteSelection2(0))throw new Exception("delete sketch");}
d.ClearSelection2(true);if(!d.Extension.SelectByRay(0,.028,-.24,0,0,1,.0001,2,false,0,0))throw new Exception("face");var sm=d.SketchManager;sm.InsertSketch(true);sm.AddToDB=true;foreach(var l in edges)sm.CreateLine(l[0],l[1],0,l[2],l[3],0);foreach(var c in circles)sm.CreateCircleByRadius(c[0],c[1],0,c[2]);
Action<double[,]> poly=pts=>{int n=pts.GetLength(0);for(int i=0;i<n;i++){int j=(i+1)%n;sm.CreateLine((128+pts[i,0])/1000,(.15+pts[i,1])/1000,0,(128+pts[j,0])/1000,(.15+pts[j,1])/1000,0);}};
poly(new double[,]{{2.15,2.2},{25.85,2.2},{25.85,33.4},{2.15,33.4}});
poly(new double[,]{{29,2},{70,2},{70,7},{73,10},{80,10},{80,30},{73,30},{70,33},{70,38.5},{29,38.5}});
double[,] holes={{4.4,36},{15.2,37},{76,36.25},{76,4.25}};for(int i=0;i<4;i++)sm.CreateCircleByRadius((128+holes[i,0])/1000,(.15+holes[i,1])/1000,0,.0017);
sm.AddToDB=false;sm.InsertSketch(true);var f=d.FeatureManager.FeatureCut3(true,false,false,0,0,.0013,.001,false,false,false,false,0,0,false,false,false,false,false,true,true,false,false,false,0,0,false);if(f==null)throw new Exception("cut failed");f.Name="Rear panel openings";d.ForceRebuild3(false);
sw.ActivateDoc3(doc.GetTitle(),false,0,ref e);var t=(double[])psu.Transform2.ArrayData;t[11]=-.224;doc.ClearSelection2(true);psu.Select4(false,null,false);ass.UnfixComponent();psu.Transform2=(MathTransform)((MathUtility)sw.GetMathUtility()).CreateTransform(t);psu.Select4(false,null,false);ass.FixComponent();doc.ClearSelection2(true);doc.ForceRebuild3(false);
var body=(Body2)((object[])((PartDoc)d).GetBodies2(0,false))[0];var faces=((object[])body.GetFaces()).Cast<Face2>().ToArray();int count=faces.Count(ff=>{var su=(Surface)ff.GetSurface();if(!su.IsCylinder())return false;var cp=(double[])su.CylinderParams;return Math.Abs(cp[5])>.999 && Math.Abs(cp[6]-.0017)<1e-7 && ((double[])ff.GetBox())[5]<-.2239;});Console.WriteLine("rear mount holes "+count);if(count!=4)throw new Exception("holes");
foreach(var model in new[]{d,doc})for(Feature ff=(Feature)model.FirstFeature();ff!=null;ff=(Feature)ff.GetNextFeature()){bool warn;if(ff.GetErrorCode2(out warn)!=0)throw new Exception(ff.Name);}
var mgr=ass.InterferenceDetectionManager;mgr.IncludeMultibodyPartInterferences=false;mgr.TreatCoincidenceAsInterference=false;var ints=(object[])mgr.GetInterferences();if(ints!=null)foreach(Interference inter in ints){Console.WriteLine("clash "+string.Join(" ",((object[])inter.Components).Cast<Component2>().Select(c=>c.Name2))+" volume mm3 "+inter.Volume*1e9);}mgr.Done();if(ints!=null&&ints.Length>0)throw new Exception("clash");
if(!d.Save3(1,ref e,ref w)||!doc.Save3(1,ref e,ref w))throw new Exception("save "+e);Console.WriteLine("saved");
}}
