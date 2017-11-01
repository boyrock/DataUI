using DataUI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour {
    
    SettingManager settingManager;
    TestSettingData data;

	// Use this for initialization
	void Start () {

        settingManager = GameObject.FindObjectOfType<SettingManager>();

        data = new TestSettingData();
        settingManager.AddSettingMenu(data, "test.json", true);
    }
	
	// Update is called once per frame
	void Update () {
		
	}
}
