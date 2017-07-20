using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using SocketIO;
using UnityStandardAssets.Vehicles.Car;
using System;
using System.Security.AccessControl;

public class CommandServer : MonoBehaviour
{
	public CarRemoteControl CarRemoteControl;
	public Camera FrontFacingCamera;
	private SocketIOComponent _socket;
	private CarController _carController;

	// Use this for initialization
	void Start()
	{
		_socket = GameObject.Find("SocketIO").GetComponent<SocketIOComponent>();
		_socket.On("open", OnOpen);//"events", callback function called when event is dequeued
//		Debug.Log (_socket.On);
		_socket.On("steer", OnSteer);
		Debug.Log ("Start up");
		_socket.On("manual", onManual);
		_carController = CarRemoteControl.GetComponent<CarController>();
	}

	// Update is called once per frame
	void Update()
	{
	}

	void OnOpen(SocketIOEvent obj)
	{
		Debug.Log("Connection Open");
		//EmitTelemetry(obj);
	}

	// 
	void onManual(SocketIOEvent obj)
	{
		//EmitTelemetry (obj);
	}

	void OnSteer(SocketIOEvent obj)
	{
		Debug.Log ("OnSteer is called:");
		JSONObject jsonObject = obj.data;
		Debug.Log ("object" + obj);
		Debug.Log ("OnSteer called:" + jsonObject.GetField("throttle").ToString());
		//    print(float.Parse(jsonObject.GetField("steering_angle").str));
		CarRemoteControl.SteeringAngle = float.Parse(jsonObject.GetField("steering_angle").ToString());
		CarRemoteControl.Acceleration = float.Parse(jsonObject.GetField("throttle").ToString());//.str did not work for us, so ToString
		//EmitTelemetry(obj);
	}

	void EmitTelemetry(SocketIOEvent obj)
	{
		UnityMainThreadDispatcher.Instance().Enqueue(() =>//new thread
		{
			print("Attempting to Send...");
			// send only if it's not being manually driven
			if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
				_socket.Emit("telemetry", new JSONObject());
			}
			else {
				// Collect Data from the Car
				Dictionary<string, string> data = new Dictionary<string, string>();
				data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
				data["throttle"] = _carController.AccelInput.ToString("N4");
				data["speed"] = _carController.CurrentSpeed.ToString("N4");
				data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
				_socket.Emit("telemetry", new JSONObject(data));
			}
		});

//		    UnityMainThreadDispatcher.Instance().Enqueue(() =>
//		    {
//		      	
//		      
//		
//				// send only if it's not being manually driven
//				if ((Input.GetKey(KeyCode.W)) || (Input.GetKey(KeyCode.S))) {
//					_socket.Emit("telemetry", new JSONObject());
//				}
//				else {
//					// Collect Data from the Car
//					Dictionary<string, string> data = new Dictionary<string, string>();
//					data["steering_angle"] = _carController.CurrentSteerAngle.ToString("N4");
//					data["throttle"] = _carController.AccelInput.ToString("N4");
//					data["speed"] = _carController.CurrentSpeed.ToString("N4");
//					data["image"] = Convert.ToBase64String(CameraHelper.CaptureFrame(FrontFacingCamera));
//					_socket.Emit("telemetry", new JSONObject(data));
//				}
//		      
//		//      
//		    });
	}
}