using System;
using System.Collections.Generic;
using System.Threading;

namespace MetaStudio;

public class RegisterHelper
{
	public static void ReadAllReg(int id)
	{
		Thread.Sleep(20);
		for (int i = 16; i <= 122; i++)
		{
			SPHelper.SendTORotor(id, 1, i, 0);
			Thread.Sleep(20);
		}
		SPHelper.SendTOStator(id, 1, 15, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 32, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 33, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 34, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 37, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 41, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 42, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 44, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 45, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 48, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 49, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 50, 0);
		Thread.Sleep(20);
		SPHelper.SendTOStator(id, 1, 51, 0);
		Thread.Sleep(20);
		SPHelper.SendTOVdbox(id, 1, 5, 0);
		Thread.Sleep(20);
		SPHelper.SendTOVdbox(id, 1, 6, 0);
		Thread.Sleep(20);
		SPHelper.SendTOVdbox(id, 1, 7, 0);
		Thread.Sleep(20);
		SPHelper.SendTOVdbox(id, 1, 34, 0);
	}

	public static void SaveConfig(int id)
	{
		SPHelper.SendTOVdbox(id, 2, 31, 0);
		SPHelper.SendTOVdbox(id, 2, 31, 1);
		SPHelper.SendTOStator(id, 2, 63, 0);
		SPHelper.SendTOStator(id, 2, 63, 1);
		SPHelper.SendTORotor(id, 2, 55, 0);
		SPHelper.SendTORotor(id, 2, 55, 1);
		Thread.Sleep(500);
		SPHelper.SendTOVdbox(id, 2, 31, 0);
		SPHelper.SendTOStator(id, 2, 63, 0);
		SPHelper.SendTORotor(id, 2, 55, 0);
	}

	public static void ImportConfig(List<RegInfo> lstReg)
	{
		Thread.Sleep(30);
		foreach (RegInfo item in lstReg)
		{
			if (item.devType == 2)
			{
				SPHelper.SendTORotor(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			else if (item.devType == 1)
			{
				SPHelper.SendTOStator(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			else if (item.devType == 0)
			{
				SPHelper.SendTOVdbox(item.deviceID, 2, Helper.Decryption(item.value1), item.value2);
			}
			Thread.Sleep(30);
		}
	}

	public static void OpenTopFusion(int id)
	{
		SPHelper.SendTOStator(id, 2, 242, 31);
		Console.WriteLine("OpenTopFusion:" + id);
	}

	public static void CloseFusion(int id)
	{
		SPHelper.SendTORotor(id, 2, 95, 0);
	}
}
