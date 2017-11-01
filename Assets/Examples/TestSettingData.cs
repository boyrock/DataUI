using DataUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace DataUI
{
    [System.Serializable]
    public class TestSettingData : SettingManager.Setting
    {
        public TestData testData;

        [ArrayLength(10)]
        public TestData[] testDataArr;

        public float v;
    }

}