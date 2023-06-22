using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WebSocketSharp;

public class socket2 : MonoBehaviour
{
    public float a;
    public float b;
    public float c;
    public float d;
    public Vector3 vectorAO;
    public Vector3 vectorBO;
    public Vector3 rotateV;
    public Vector3 rotateV2;
    public Vector3 posM1;
    public Vector3 posM2;
    public string ip;
    public Vector3 vectorAO1;
    public Vector3 vectorAO2;
    WebSocket ws;

    public static bool IsIntercept(Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        return (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f);
    }

    public static bool LineLineIntersection(out Vector3 intersection, Vector3 linePoint1, Vector3 lineVec1, Vector3 linePoint2, Vector3 lineVec2)
    {
        Vector3 lineVec3 = linePoint2 - linePoint1;
        Vector3 crossVec1and2 = Vector3.Cross(lineVec1, lineVec2);
        Vector3 crossVec3and2 = Vector3.Cross(lineVec3, lineVec2);

        float planarFactor = Vector3.Dot(lineVec3, crossVec1and2);

        //is coplanar, and not parrallel
        if (Mathf.Abs(planarFactor) < 0.0001f && crossVec1and2.sqrMagnitude > 0.0001f)
        {
            float s = Vector3.Dot(crossVec3and2, crossVec1and2) / crossVec1and2.sqrMagnitude;
            intersection = linePoint1 + (lineVec1 * s);
            return true;
        }
        else
        {
            intersection = Vector3.zero;
            return false;
        }
    }
    // Start is called before the first frame update
    void Start()
    {
        rotateV = new Vector3(0, 0, 0);
        rotateV2 = new Vector3(0, 0, 0);
        posM1 = new Vector3(0, 10, 0);
        posM2 = new Vector3(10, 10, 0);

        ws = new WebSocket($"ws://{ip}");
        ws.Connect();
        ws.OnMessage += (sender, e) =>
        {
            var correct = false;
            var startPos = -1;
            while (!correct)
            {
                startPos++;
                //Debug.Log("Message Received from " + ((WebSocket)sender).Url + ", Data : " + e.Data);
                var arr = e.Data.Split('/');
                a = Convert.ToSingle(arr[startPos + 0].Replace('.', ','));
                b = Convert.ToSingle(arr[startPos + 1].Replace('.', ','));
                c = Convert.ToSingle(arr[startPos + 2].Replace('.', ','));
                d = Convert.ToSingle(arr[startPos + 3].Replace('.', ','));
                vectorAO1 = new Vector3(0, (float)Math.Cos((a * (Math.PI)) / 180), -1 * (float)Math.Sin((a * (Math.PI)) / 180));
                vectorAO2 = new Vector3((float)Math.Sin((b * (Math.PI)) / 180), (float)Math.Cos((a * (Math.PI)) / 180), 0);
                vectorAO = Vector3.Cross(vectorAO1, vectorAO2);
                var vectorBO1 = new Vector3(0, (float)Math.Cos((c * (Math.PI)) / 180), -1 * (float)Math.Sin((c * (Math.PI)) / 180));
                var vectorBO2 = new Vector3((float)Math.Sin((d * (Math.PI)) / 180), (float)Math.Cos((d * (Math.PI)) / 180), 0);
                vectorBO = Vector3.Cross(vectorBO1, vectorBO2);
                correct = IsIntercept(posM1, vectorAO, posM2, vectorBO);
                if (startPos == 3)
                    break;
            }
            vectorBO = Quaternion.Euler(rotateV) * vectorBO;
            vectorAO = Quaternion.Euler(rotateV2) * vectorAO;
        };

    }
    void OnApplicationQuit()
    {
        //   ws.Close();
        Debug.Log("Application ending after " + Time.time + " seconds");
    }
    // Update is called once per frame
    void Update()
    {
        Debug.DrawRay(posM1, 10 * vectorAO, Color.red);
        Debug.DrawRay(posM2, 10 * vectorBO, Color.green);
        LineLineIntersection(out var resultV, posM1, vectorAO, posM2, vectorBO);
        Debug.Log($"resultPoint {resultV}");
        transform.position = resultV;
        if (ws == null)
        {
            return;
        }
    }
}
