namespace AC
{
	public struct SpeechGap
	{
		public int characterIndex;

		public float waitTime;

		public bool pauseIsIndefinite;

		public int expressionID;

		public string tokenKey;

		public string tokenValue;

		public SpeechGap(int _characterIndex, float _waitTime)
		{
			characterIndex = _characterIndex;
			waitTime = _waitTime;
			expressionID = -1;
			pauseIsIndefinite = false;
			tokenKey = (tokenValue = string.Empty);
		}

		public SpeechGap(int _characterIndex, bool _pauseIsIndefinite)
		{
			characterIndex = _characterIndex;
			waitTime = -1f;
			expressionID = -1;
			pauseIsIndefinite = _pauseIsIndefinite;
			tokenKey = (tokenValue = string.Empty);
		}

		public SpeechGap(int _characterIndex, int _expressionID)
		{
			characterIndex = _characterIndex;
			waitTime = -1f;
			expressionID = _expressionID;
			pauseIsIndefinite = false;
			tokenKey = (tokenValue = string.Empty);
		}

		public SpeechGap(int _characterIndex, string _tokenKey, string _tokenValue)
		{
			characterIndex = _characterIndex;
			waitTime = 0f;
			expressionID = -1;
			tokenKey = _tokenKey;
			tokenValue = _tokenValue;
			pauseIsIndefinite = false;
		}

		public void CallEvent(Speech speech)
		{
			if (!string.IsNullOrEmpty(tokenValue))
			{
				KickStarter.eventManager.Call_OnSpeechToken(speech, tokenKey, tokenValue);
			}
		}
	}
}
