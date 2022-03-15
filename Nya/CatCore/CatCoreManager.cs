using System;
using CatCore;
using CatCore.Services.Multiplexer;
using HMUI;
using Nya.Configuration;
using Nya.Utils;
using SiraUtil.Logging;
using Zenject;

namespace Nya.CatCore
{
	internal class CatCoreManager : IInitializable, IDisposable

	{
		private DateTime _nyaCommandUsedTime;
		
		private readonly SiraLog _siraLog;
		private readonly ImageUtils _imageUtils;
		private readonly CatCoreInfo _catCoreInfo;
		private readonly PluginConfig _pluginConfig;
		private readonly CatCoreInstance _catCoreInstance;
		private ChatServiceMultiplexer? _chatServiceMultiplexer;

		public CatCoreManager(SiraLog siraLog, ImageUtils imageUtils, CatCoreInfo catCoreInfo, PluginConfig pluginConfig)
		{
			_siraLog = siraLog;
			_imageUtils = imageUtils;
			_catCoreInfo = catCoreInfo;
			_pluginConfig = pluginConfig;
			_catCoreInstance = CatCoreInstance.Create();
		}

		private void CatService_OnTextMessageReceived(MultiplexedPlatformService service, MultiplexedMessage message)
		{
			try
			{
				var messageContent = message.Message.ToLower();
				if (messageContent == "!nya")
				{
					_siraLog.Info("Received Nya command");
					if (_catCoreInfo.CurrentImageView != null)
					{
						if (_nyaCommandUsedTime.AddSeconds(_pluginConfig.NyaCommandCooldown) < DateTime.Now)
						{
							//_imageUtils.LoadNewNyaImage(_catCoreInfo.CurrentImageView);
							_nyaCommandUsedTime = DateTime.Now;
							message.Channel.SendMessage("! 😸");
						}
						else
						{
							message.Channel.SendMessage("! Nya is on cooldown! 🙀");	
						}
					}
					else
					{
						message.Channel.SendMessage("! Nya isn't active! 😿");
					}
				}

				if (messageContent == "!currentnya" || messageContent == "!bangernya")
				{
					_siraLog.Info("Received CurrentNya command");
					if (_imageUtils.NyaImageURL != null)
					{
						message.Channel.SendMessage("! " + _imageUtils.NyaImageURL);
					}
					else
					{
						message.Channel.SendMessage("! Couldn't get the current image 😿");
					}
				}
			}
			catch (Exception e)
			{
				_siraLog.Critical(e);
			}
		}

		public void StartCatCoreServices()
		{
			_chatServiceMultiplexer = _catCoreInstance.RunAllServices();
			_chatServiceMultiplexer!.OnTextMessageReceived += CatService_OnTextMessageReceived;
		}

		public void EndCatCoreServices()
		{
			if (_chatServiceMultiplexer != null)
			{
				_chatServiceMultiplexer!.OnTextMessageReceived -= CatService_OnTextMessageReceived;
				_chatServiceMultiplexer = null;
			}

			_catCoreInstance.StopAllServices();
		}

		public void Initialize()
		{
			if (_pluginConfig.CatCoreEnabled)
			{
				StartCatCoreServices();
			}
		}

		public void Dispose() => EndCatCoreServices();
	}
}