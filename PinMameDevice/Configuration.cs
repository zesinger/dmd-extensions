﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using IniParser;
using IniParser.Model;
using Microsoft.Win32;
using NLog;

namespace PinMameDevice
{
	public class Configuration
	{
		public readonly VirtualDmdConfig VirtualDmd;
		public readonly PinDmd1Config PinDmd1;
		public readonly PinDmd2Config PinDmd2;
		public readonly PinDmd3Config PinDmd3;
		public readonly Pin2DmdConfig Pin2Dmd;

		private readonly string _iniPath;
		private readonly FileIniDataParser _parser;
		private readonly IniData _data;

		protected static readonly Logger Logger = LogManager.GetCurrentClassLogger();

		public Configuration()
		{
			var assemblyPath = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
			_iniPath = Path.Combine(assemblyPath, "DmdDevice.ini");
			_parser = new FileIniDataParser();
			_data = File.Exists(_iniPath) ? _parser.ReadFile(_iniPath) : new IniData();

			VirtualDmd = new VirtualDmdConfig(_data, this);
			PinDmd1 = new PinDmd1Config(_data, this);
			PinDmd2 = new PinDmd2Config(_data, this);
			PinDmd3 = new PinDmd3Config(_data, this);
			Pin2Dmd = new Pin2DmdConfig(_data, this);
		}

		public void Save()
		{
			Logger.Info("Saving config to {0}", _iniPath);
			_parser.WriteFile(_iniPath, _data);
		}
	}

	public class PinDmd1Config : AbstractConfiguration
	{
		public override string Name { get; } = "pindmd1";

		public bool Enabled
		{
			get { return GetBoolean("enabled", true); }
			set { Set("enabled", value); }
		}

		public PinDmd1Config(IniData data, Configuration parent) : base(data, parent)
		{
		}
	}

	public class PinDmd2Config : AbstractConfiguration
	{
		public override string Name { get; } = "pindmd2";

		public bool Enabled
		{
			get { return GetBoolean("enabled", true); }
			set { Set("enabled", value); }
		}

		public PinDmd2Config(IniData data, Configuration parent) : base(data, parent)
		{
		}
	}

	public class PinDmd3Config : AbstractConfiguration
	{
		public override string Name { get; } = "pindmd3";

		public bool Enabled
		{
			get { return GetBoolean("enabled", true); }
			set { Set("enabled", value); }
		}

		public string Port
		{
			get { return GetString("port", ""); }
			set { Set("port", value); }
		}

		public PinDmd3Config(IniData data, Configuration parent) : base(data, parent)
		{
		}
	}

	public class Pin2DmdConfig : AbstractConfiguration
	{
		public override string Name { get; } = "pin2dmd";

		public bool Enabled
		{
			get { return GetBoolean("enabled", true); }
			set { Set("enabled", value); }
		}

		public Pin2DmdConfig(IniData data, Configuration parent) : base(data, parent)
		{
		}
	}

	public class VirtualDmdConfig : AbstractConfiguration
	{
		public override string Name { get; } = "virtualdmd";

		public bool Enabled
		{
			get { return GetBoolean("enabled", true); }
			set { Set("enabled", value); }
		}

		public double Left
		{
			get { return GetDouble("left", 0); }
			set { Set("left", value); }
		}

		public double Top
		{
			get { return GetDouble("top", 0); }
			set { Set("top", value); }
		}

		public double Width
		{
			get { return GetDouble("width", 1024); }
			set { Set("width", value); }
		}

		public double Height
		{
			get { return GetDouble("height", 256); }
			set { Set("height", value); }
		}

		public VirtualDmdConfig(IniData data, Configuration parent) : base(data, parent)
		{
		}

		public void SetPosition(double left, double top, double width, double height)
		{
			DoWrite = false;
			Left = left;
			Top = top;
			Width = width;
			Height = height;
			Save();
		}
	}

	public abstract class AbstractConfiguration
	{
		public abstract string Name { get; }
		private readonly IniData _data;
		private readonly Configuration _parent;

		protected bool DoWrite = true;

		protected AbstractConfiguration(IniData data, Configuration parent)
		{
			_parent = parent;
			_data = data;
		}

		protected void Save()
		{
			DoWrite = true;
			_parent.Save();
		}
		protected void Set(string key, bool value)
		{
			if (_data[Name] == null) {
				_data.Sections.Add(new SectionData(Name));
			}
			_data[Name][key] = value ? "true" : "false";
			if (DoWrite) {
				_parent.Save();
			}
		}

		protected bool GetBoolean(string key, bool fallback)
		{
			if (_data[Name] == null) {
				return fallback;
			}

			try {
				return bool.Parse(_data[Name][key]);
			} catch (FormatException e) {
				throw new InvalidIniValueException("Value \"" + _data[Name][key] + "\" for \"" + key + "\" under [" + Name + "] must be either \"true\" or \"false\".");
			}
		}

		protected void Set(string key, int value)
		{
			if (_data[Name] == null) {
				_data.Sections.Add(new SectionData(Name));
			}
			_data[Name][key] = value.ToString();
			if (DoWrite) {
				_parent.Save();
			}
		}

		protected int GetInt(string key, int fallback)
		{
			if (_data[Name] == null || !_data[Name].ContainsKey(key)) {
				return fallback;
			}

			try {
				return int.Parse(_data[Name][key]);
			} catch (FormatException e) {
				throw new InvalidIniValueException("Value \"" + _data[Name][key] + "\" for \"" + key + "\" under [" + Name + "] must be an integer.");
			}
		}

		protected void Set(string key, double value)
		{
			if (_data[Name] == null) {
				_data.Sections.Add(new SectionData(Name));
			}
			_data[Name][key] = value.ToString();
			if (DoWrite) {
				_parent.Save();
			}
		}

		protected double GetDouble(string key, double fallback)
		{
			if (_data[Name] == null || !_data[Name].ContainsKey(key)) {
				return fallback;
			}

			try {
				return double.Parse(_data[Name][key]);
			} catch (FormatException e) {
				throw new InvalidIniValueException("Value \"" + _data[Name][key] + "\" for \"" + key + "\" under [" + Name + "] must be a floating number.");
			}
		}

		protected void Set(string key, string value)
		{
			if (_data[Name] == null) {
				_data.Sections.Add(new SectionData(Name));
			}
			_data[Name][key] = value;
			if (DoWrite) {
				_parent.Save();
			}
		}

		protected string GetString(string key, string fallback)
		{
			if (_data[Name] == null || !_data[Name].ContainsKey(key)) {
				return fallback;
			}
			return _data[Name][key];
		}
	}

	public class InvalidIniValueException : Exception
	{
		public InvalidIniValueException(string message) : base(message)
		{
		}
	}
}