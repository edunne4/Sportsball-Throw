using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimeControl {

	//public float slowDownFactor = 0.1f;

	public static void StartSlowMotion(float slowDownFactor){
		Time.timeScale = slowDownFactor;
		Time.fixedDeltaTime = Time.timeScale * 0.02f;
	}

	public static void EndSlowMotion(){
		Time.timeScale = 1;
		Time.fixedDeltaTime = 0.02f;
	}



	public static IEnumerator SlowDownCo(float duration){
        StartSlowMotion(0.1f);
		yield return new WaitForSeconds (duration);
        EndSlowMotion();
	}
}
