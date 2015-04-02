package sysnetlab.android.sdc.replay.test;

import java.io.InputStream;
import java.util.List;

import name.bagi.levente.pedometer.StepDetector;
import name.bagi.levente.pedometer.StepListener;
import name.bagi.levente.pedometer.StepService;
import sysnetlab.android.sdc.replay.SensingAssertionEnforcer;
import sysnetlab.android.sdc.replay.MockSensingContext;
import sysnetlab.android.sdc.replay.SenSeeSensingAssertionEnforcer;
import sysnetlab.android.sdc.replay.SenSeeSensingReplayer;
import sysnetlab.android.sdc.replay.SensingReplayer;
import sysnetlab.android.sdc.replay.metamorphic.MetamorphicTransform;
import sysnetlab.android.sdc.replay.metamorphic.NoEffectMetamorphicTransform;

import android.content.Context;
import android.content.Intent;
import android.hardware.Sensor;
import android.hardware.SensorEventListener;
import android.test.ServiceTestCase;

public class StepServiceTest 
		extends ServiceTestCase<StepService> {

    private int stepsCountedByStepDetector = 0;

	public StepServiceTest() 
	{
		super(StepService.class);
	}

	public void setUp() throws Exception {
	}

	public void tearDown() throws Exception {
	}
	
	public void testReplayInStepService() throws Exception 
	{
        //set up mock context to capture the application's sensor data listener
		MockSensingContext mockContext = new MockSensingContext(getContext());
        setContext(mockContext);

        //start the StepService
        startService(new Intent());     
        StepService stepService = (StepService) getService();
        assertNotNull(stepService);      

        //set up the replayer, using files from SenSee and the captured listener
        SensorEventListener capturedListener = mockContext.getCapturedEventListener();
        assertNotNull(capturedListener);
        StepDetector stepDetector = (StepDetector) capturedListener;
        assertNotNull(stepDetector);

        Context testContext = (Context)getClass().getMethod("getTestContext").invoke(this);
        InputStream dataStream = testContext.getAssets().open("accelerometer.csv");
        int stepsInSenSeeExperiment = 25;

        SensingReplayer replayer = new SenSeeSensingReplayer(dataStream, capturedListener);

        //set up a step listener to detect when the application counts a step
        stepDetector.addStepListener(new StepListener() {
            @Override
            public void onStep() {
                stepsCountedByStepDetector++;
            }

            @Override
            public void passValue() {
                //do nothing
            }
        });

        //replay
        List<float[]> allSensorValues = replayer.getAllSensorValuesForReplay();
        replayInStepDetectorAndAssert(stepDetector, stepsInSenSeeExperiment, allSensorValues);

        //transform and replay again
        MetamorphicTransform transformOne = new NoEffectMetamorphicTransform();
        List<float[]> morphedSensorValues = transformOne.transform(allSensorValues);
        replayInStepDetectorAndAssert(stepDetector, stepsInSenSeeExperiment, morphedSensorValues);


	}

    private void replayInStepDetectorAndAssert(StepDetector stepDetector,
                                               int stepsInSenSeeExperiment,
                                               List<float[]> morphedSensorValues) {

        for(float[] sensorValues : morphedSensorValues)
        {
            stepDetector.onSensorChanged(Sensor.TYPE_ACCELEROMETER, sensorValues);
        }

        double errorThreshold = (stepsInSenSeeExperiment * .2); //80% accuracy
        assertTrue(Math.abs(stepsCountedByStepDetector - stepsInSenSeeExperiment) <= errorThreshold);
    }

}
