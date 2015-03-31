using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ABB.SrcML;
using ABB.SrcML.Data;
using NUnit.Framework;

namespace CodeAnalysisToolkit
{
    [TestFixture]
    public class SimpleAnalyticsCalculator_Thesis_v2
    {
        [TestCase]
        public void CalculateSimpleProjectStats()
        {

            //Dump output to text file name "out.txt" 
            //var file = new StreamWriter("out.txt");

            // Specify the loaction of the files you would like to parse below

            var dataProject = new DataProject<CompleteWorkingSet>("android-pedometer-studio",
                Path.GetFullPath(@"C:/School/Grad School (Comp Sci)/Thesis/android-pedometer-studio"),
                "..//..//..//SrcML");
            
            /*
            var dataProject = new DataProject<CompleteWorkingSet>("Pedometer-master",
                Path.GetFullPath(@"C:\School\Grad School (Comp Sci)\Thesis\Apps\Pedometer-master"),
                "..//..//..//SrcML");
            */


            dataProject.UpdateAsync().Wait();

            NamespaceDefinition globalNamespace;
            Assert.That(dataProject.WorkingSet.TryObtainReadLock(5000, out globalNamespace));




            // As of right now the test fails if you include the below code that uses the "MethodCall" Function

            //-----Start of failing code------------------------------------
            var SensorEventListener = from method in globalNamespace.GetDescendants<MethodCall>()
                                      where method.Name == "SensorEventListener"
                                      select method;


            Debug.WriteLine("-- ");

            Debug.WriteLine(SensorEventListener.Count() + " Implementations of " + SensorEventListener.ElementAt(0));

            Debug.WriteLine("-- ");

            int k = SensorEventListener.Count();

            //------End of Failing code-------------------------------------

            //------The Below code is me trying to replacate what we did at the bottom, but we need the "MethodCall" Function not to fail 1st

            /*
            for (int i = 0; i < k; i++)
            {
                var senChangedMethod = SensorEventListener.ElementAt(i);
                Debug.WriteLine("Implementations of onSensorChaged:  " + (i + 1) + " " + senChangedMethod.Name);
                var callsToSenEvent = SensorEventListener.GetCallsToSelf();

                for (int j = 0; j < callsToSenEvent.Count(); j++)
                {
                    var callerMethod = callsToSenEvent.ElementAt(j).ParentStatement.GetAncestorsAndSelf<MethodCall>();

                    if (callerMethod.Any())
                    {
                        Debug.WriteLine("Called by " + callerMethod.ElementAt(0).GetFullName());
                    }

                }
            }
             */

            //-----End of me trying to replicate what we did below in the working code.


            Debug.WriteLine("#####");
            Debug.WriteLine("#####");


            //---Start of working Code----------------------------------------------------

            var senChangedMethods = from method in globalNamespace.GetDescendants<MethodDefinition>()
                                     where method.Name == "onSensorChanged"
                                     select method;


            Debug.WriteLine("----- ");

            Debug.WriteLine("\r\n");

            Debug.WriteLine(senChangedMethods.Count() + " Implementations of " + senChangedMethods.First().GetFullName());

            Debug.WriteLine("----- ");

            int n = senChangedMethods.Count();

            for (int i = 0; i < n; i++){

                var senChangedMethod = senChangedMethods.ElementAt(i);

                Debug.WriteLine("Implementations of onSensorChaged # " + (i + 1) + ": " + senChangedMethod.GetFullName());

                //"GetCallsToSelf" returns the number of times the number is called
                var callsToSenChanged = senChangedMethod.GetCallsToSelf();

                for (int j = 0; j < callsToSenChanged.Count(); j++)
                {
                    var callerMethod = callsToSenChanged.ElementAt(j).ParentStatement.GetAncestorsAndSelf<MethodDefinition>();

                    if (callerMethod.Any())
                    {
                        Debug.WriteLine("   Called by --> " + callerMethod.ElementAt(0).GetFullName());
                    }

                }
                Debug.WriteLine("----- ");
            }




            //---------------------------------------------------------------------

        }


        public string endl { get; set; }
    }
}
