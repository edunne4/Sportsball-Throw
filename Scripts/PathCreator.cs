using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PathCreator : MonoBehaviour {

	public float distBetweenPoints = 1f;
    public int numPoints = 25;

	public LineRenderer line;

	private List<Vector3> points = new List<Vector3> ();
    private GameplayManager gpm;

	void Awake (){
		line = GetComponent<LineRenderer> ();
	}

	// Use this for initialization
	void Start () {
        gpm = GameplayManager.instance;

	}
	
	// Update is called once per frame
	void Update () {
        if (gpm.curState == GameplayManager.GameState.PLAY_NOT_STARTED)
        {//qb.hasHiked) {

            if (Input.touchCount > 0)
            {
                Touch touch = Input.GetTouch(0);
                switch (touch.phase)
                {
                    case TouchPhase.Began:
                        points.Clear(); //deletes previous path (even on hike button touch, fix this)
                        Vector2 newPoint = Camera.main.ScreenToWorldPoint(touch.position);
                        points.Add(newPoint);
                        line.enabled = true;
                        break;
                    case TouchPhase.Moved:
                        newPoint = Camera.main.ScreenToWorldPoint(touch.position);
                        if (points.Count < numPoints)     //check for limit
                        {
                            if (GetDistanceToLastPoint(newPoint) > distBetweenPoints)
                            {
                                points.Add(newPoint);
                                line.positionCount = points.Count;
                                line.SetPositions(points.ToArray());
                            }
                        }

                        break;
                    case TouchPhase.Ended:
                        Vector2 finalPoint = Camera.main.ScreenToWorldPoint(touch.position);
                        if (points.Count < numPoints)     //check for limit
                        {
                            points.Add(finalPoint);
                            line.positionCount = points.Count;
                        }
                        line.SetPositions(points.ToArray());

                        //give path to receiver
                        gpm.receiver.SetRoute(points);
                        break;
                }
            }
        }

	}


	float GetDistanceToLastPoint(Vector2 newPoint){
		if (points.Count > 0) {
			return Vector2.Distance (points.Last(), newPoint);
		} else {
			return Mathf.Infinity;
		}
	}

	public void RemovePath(){
		points.Clear ();
		line.positionCount = points.Count;
		line.SetPositions (points.ToArray());
	}
		
}
