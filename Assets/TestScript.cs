using System;
using System.Collections;
using System.Collections.Generic;
using MiniJSON;
using UnityEngine;
using UnityEngine.UI;
using BestHTTP;


public class TestScript : MonoBehaviour {
	public bool Reset = false;
	public string URL;
	public string Path;

	public void Awake (){
		HTTPRequest request = new HTTPRequest (new Uri (URL+Path), HTTPMethods.Post, OnRequestFinished);
		request.AddField ("name", "akr");
		request.AddField ("password",StringUtils.GeneratePassword(30));
		request.Send ();
	}
	void Update () {
		if(Reset){
			Reset = false;
			Awake ();
		}
	}

	void OnRequestFinished(HTTPRequest request, HTTPResponse response) {
		
		Debug.Log("Request Finished! Text received: " + response.DataAsText);
	}
	public static class StringUtils
	{
		private const string PASSWORD_CHARS = 
			"0123456789abcdefghijklmnopqrstuvwxyz";

		public static string GeneratePassword( int length )
		{
			var sb  = new System.Text.StringBuilder( length );
			var r   = new System.Random();

			for ( int i = 0; i < length; i++ )
			{
				int     pos = r.Next( PASSWORD_CHARS.Length );
				char    c   = PASSWORD_CHARS[ pos ];
				sb.Append( c );
			}

			return sb.ToString();
		}
	}





}
