/*
 * A class to write a log file for the modules with events useful for
 * debugging and administration. The file produced is a trace file, not
 * to be confused with the log files that modules produce with their
 * output data.
 */

using UnityEngine;
using System.IO;

public class NeuroLog
{
	/*
	 * Before log in use machine-wide log file:
	 * machine_dashboard_trace.txt
	 */
	private static string log_fn = Path.Combine(XmlManager.TraceFilesPath,
												System.Environment.MachineName + "_trace.txt");


	/*
	 * Set the player id that we'll use. From this point on the log file will
	 * be called player_machine_trace.txt.
	 */
	public static void SetPlayer(string player_id)
	{
		log_fn = Path.Combine(XmlManager.TraceFilesPath,
				 player_id + "_" + System.Environment.MachineName + "_trace.txt");
		UnityEngine.Debug.Log("using trace file " + log_fn);
		Log("Using player name " + player_id);
	}


	/*
	 * Log a message with a timestamp
	 */
	private static void _Log(string format, params System.Object[] args)
	{
		using (StreamWriter sw = new StreamWriter(log_fn, true)) {
			string msg = System.String.Format(format, args);
			string log_msg = System.String.Format("{0:G}: MODULE: {1}", System.DateTime.Now, msg);
			sw.WriteLine(log_msg);
			UnityEngine.Debug.Log(log_msg);
		}
	}


	/*
	 * Log a normal message
	 */
	public static void Log(string format, params System.Object[] args)
	{
		_Log("INFO: " + format, args);
	}


	/*
	 * Log an error message
	 */
	public static void Error(string format, params System.Object[] args)
	{
		_Log("ERROR: " + format, args);
	}


	/*
	 * Log a debug message
	 */
	public static void Debug(string format, params System.Object[] args)
	{
		_Log("DEBUG: " + format, args);
	}
}
