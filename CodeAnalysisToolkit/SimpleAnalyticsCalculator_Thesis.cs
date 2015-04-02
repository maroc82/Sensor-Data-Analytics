﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    public class SimpleAnalyticsCalculator_Thesis
    {
        [TestCase]
        public void CalculateSimpleProjectStats()
        {         
            var dataProject = new DataProject<CompleteWorkingSet>("android-pedometer-studio",
                Path.GetFullPath("..//..//..//projects//android-pedometer-studio"),
                "..//..//..//SrcML");           

            Debug.WriteLine("Parsing android-pedometer-studio....");

            dataProject.UpdateAsync().Wait();

            NamespaceDefinition globalNamespace;
            Assert.That(dataProject.WorkingSet.TryObtainReadLock(5000, out globalNamespace));

            DisplaySensorTypes(globalNamespace);
            //DisplayWhetherAppIsUnitTested();           
            DisplayCallsToOnSensorChanged(globalNamespace);            
        }

        private void DisplaySensorTypes(NamespaceDefinition globalNamespace)
        {
            var getDefaultSensorCalls = from statement in globalNamespace.GetDescendantsAndSelf()
                                        from expression in statement.GetExpressions()
                                        from call in expression.GetDescendantsAndSelf<MethodCall>()                                     
                                        where call.Name == "getDefaultSensor"
                                        select call;

            foreach (var call in getDefaultSensorCalls)
            {
                if (call.Arguments.Any())
                {
                    var firstArg = call.Arguments.First();
                    var components = firstArg.Components;
                    if (components.Count() == 3 &&
                        components.ElementAt(0).ToString() == "Sensor" &&
                        components.ElementAt(1).ToString() == ".")
                    {
                        Debug.WriteLine("sensor " + components.ElementAt(2).ToString() + " found");
                    }
                }
            }
        }

        private void DisplayWhetherAppIsUnitTested()
        {
            throw new NotImplementedException();
        }

        private void DisplayCallsToOnSensorChanged(NamespaceDefinition globalNamespace)
        {
            var senChangedMethods = from method in globalNamespace.GetDescendants<MethodDefinition>()
                where method.Name == "onSensorChanged"
                select method;

            Debug.WriteLine("#####");
            Debug.WriteLine("#####");
            Debug.WriteLine("----- ");
            Debug.WriteLine("\r\n");
            Debug.WriteLine(senChangedMethods.Count() + " Implementations of " + senChangedMethods.First().GetFullName());
            Debug.WriteLine("----- ");

            int n = senChangedMethods.Count();
            for (int i = 0; i < n; i++)
            {
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
        }
    }
}
