using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CodeManager : MonoBehaviour
{
    public static List<string> activeCodes = new List<string>();

    System.Random rand = new System.Random();

    string letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    string numbers = "0123456789";

 private void Start() 
 { 
     
 }

    public void MakeCode()
    {
        print(GenerateCode());
    }

    #region |Generation
    public string GenerateString(int size, string alphabet)
    {
        char[] chars = new char[size];
        for (int i = 0; i < size; i++)
        {
            chars[i] = alphabet[rand.Next(alphabet.Length)];
        }
        return new string(chars);
    }

    public ushort GenerateCode()
    {
        string code = GenerateString(3, letters) + GenerateString(3, numbers);
        int failsafeLoopCount = 0;

        while (CodeManager.activeCodes.Contains(code))
        {
            code = GenerateString(3, letters) + GenerateString(3, numbers);
            print("looped");

            failsafeLoopCount++;
            if (failsafeLoopCount > 5)
            {
                Debug.LogError("**ERROR**\nToo many games running simultaneously!!");
                break;
            }
        }

        ActivateCode(code);
        ushort uCode = Convert.ToUInt16(code);
        return uCode;
    }
    #endregion

    #region |Activation
    public void ActivateCode(string code)
    {
        if (!CodeManager.activeCodes.Contains(code))
            CodeManager.activeCodes.Add(code);
        else
            Debug.LogError("**ERROR**\nYou are trying to activate a code that is already being used!!");
    }

    public static void DeactivateCode(string code)
    {
        if (CodeManager.activeCodes.Contains(code))
            CodeManager.activeCodes.Remove(code);
        else
            Debug.LogError("**ERROR**\nYou are trying to deactivate a code that is not active!!");
    }

    public static bool IsCodeActive(string code)
    {
        if (CodeManager.activeCodes.Contains(code))
            return true;
        else
            return false;
    }
    #endregion
}