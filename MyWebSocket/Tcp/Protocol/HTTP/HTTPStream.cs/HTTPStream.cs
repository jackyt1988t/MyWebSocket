﻿using System;

namespace MyWebSocket.Tcp.Protocol.HTTP
{
	class HTTPStream : StreamS
	{
		const int LF = 0x0A;
		const int CR = 0x0D;
		const int CN = 0x3A;
		const int SPACE = 0x20; 
		const int STSTR = 1024;
		const int PARAM = 1024;
		const int VALUE = 1024;

		public static readonly byte[] ENDCHUNCK;
		public static readonly byte[] EOFCHUNCK;

		public IHeader header;
		public HTTPFrame _Frame;

		static HTTPStream()
		{
			ENDCHUNCK =
				new byte[] { 0x0D, 0x0A };
			EOFCHUNCK = 
				new byte[] { 0x30, 0x0D, 0x0A, 0x0D, 0x0A };
		}
		public HTTPStream(int length) :
			base(length)
		{
			_Frame = new HTTPFrame();
		}

		public override int ReadBody()
		{
			if (_Frame.Handl == 0)
			{
				header.SetEnd();
				_Frame.GetBody = true;
				return 0;
			}

			int read = 0;
			int @char = 0;
			while (!Empty)
			{
				@char = Buffer[PointR];
				switch ( _Frame.Handl )
				{
					case 1:
						header._Body[_Frame.bpart] = (byte)@char;
						break;
					case 2:
						if (@char == CR)
						{
							_Frame.Handl = 3;
							break;
						}
						else
							_Frame.Param += char.ToLower((char)@char);
						break;
					case 3:
						if (@char != LF)
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						if (!int.TryParse(_Frame.Param, out _Frame.bleng))
							throw new HTTPException("Неверная длинна тела.", HTTPCode._400_);
						
						_Frame.Handl = 1;
						header._Body = new byte[_Frame.bleng];
						break;
					case 4:
						if (@char == CR)
							_Frame.Handl = 5;
						else
							throw new HTTPException("отсутсвует символ[CR]", HTTPCode._400_);
						break;
					case 5:
						if (@char == LF)
						{
							_Frame.Pcod = 1;
							_Frame.Handl = 6;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);
						break;
					case 6:
						if (@char == CR)
							_Frame.Handl = 7;
						else
						{
							_Frame.Param += char.ToLower((char)@char);
							return read;
						}
						break;
					case 7:
						if (@char == LF)
						{
							_Frame.Pcod = 0;
							return read;
						}
						else
							throw new HTTPException("отсутсвует символ[LF]", HTTPCode._400_);

				}
				read++;
				PointR++;
				_Frame.bpart++;

				if (_Frame.bpart == _Frame.bleng)
				{
					header.SegmentsBuffer.Enqueue(header._Body);
					if (!string.IsNullOrEmpty(  _Frame.Param  ))
						_Frame.Handl = 4;
					else
					{
						_Frame.GetBody = true;
						return read;
					}
				}
			}
			read = -1;
			return read;
		}
		/// <summary>
		/// считывает из потока заголовки http запроса и записывает их в header
		/// </summary>
		/// <returns>-1 если заголвки не были получены</returns>
		/// <exception cref="HTTPException"></exception>
		/// <exception cref="HeadersException"></exception>

		public override int ReadHead()
		{
			int read = 0;
			int @char = 0;
			
			while (!Empty)
			{
				if ( _Frame.hleng > 36000 )
					throw new HTTPException( "Превышена длинна заголовков", HTTPCode._400_ );

				@char = Buffer[PointR];
				switch ( _Frame.Handl )
				{
					// получает стартовую строку
					case 0:
						if (_Frame.ststr > STSTR)
							throw new HTTPException( "Длинна стартовой строки", HTTPCode._400_ );
						if (@char == CR)
						{
							_Frame.Handl = 4;
							header.StartString = _Frame.StStr;
								   _Frame.StStr = string.Empty;
						}
						else
						{
							_Frame.ststr++;
							_Frame.StStr += char.ToLower((char)@char);
							if (@char == SPACE)
							{
								_Frame.Hand++;
								if (_Frame.Hand == 2)
									ParsePath( header.Path, header );
									if (_Frame.Hand > 2)
										header.Http +=
										   char.ToLower((char)@char);
						}
							else
							{
								switch (_Frame.Hand)
								{
									case 1:
										header.Path +=
										   char.ToLower((char)@char);
										break;
									case 2:
										header.Http +=
										   char.ToLower((char)@char);
										break;
									case 0:
										header.Method +=
										   char.ToUpper((char)@char);
										break;
								}
							}
						}
						break;
					// получает параметр заголовка
					case 1:
						if (_Frame.param > PARAM)
							throw new HTTPException( "Длинна параметра заголовка", HTTPCode._400_ );
						if (@char == CN)
						{
							PointR++;
                            _Frame.Handl = 2;
						}
						else
						{
							_Frame.param++;
							_Frame.Param += char.ToLower((char)@char);
						}
						break;
					// получает значение заголовка
					case 2:
						if (_Frame.value > VALUE)
							throw new HTTPException( "Длинна значения заголовка", HTTPCode._400_ );
						if (@char == CR)
						{
							_Frame.param = 0;
							_Frame.value = 0;
							_Frame.Handl = 4;
							header.AddHeader(_Frame.Param, _Frame.Value);
							_Frame.Param = string.Empty;
							_Frame.Value = string.Empty;
						}
						else
						{
							_Frame.value++;
							_Frame.Value += ( char )@char;
						}
						break;
					// проверяет получены или нет заголвоки
					case 3:
						if (@char == CR)
							_Frame.Handl = 5;
						else
						{
							_Frame.Handl = 1;
							if (@char == CN)
                            	_Frame.Handl = 2;
							else
							{
								_Frame.param++;
								_Frame.Param += char.ToLower((char)@char);
							}
						}
						break;
					// проверяет правильность окончания заголовка
					case 4:
						if (@char == LF)
							_Frame.Handl = 3;
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						break;
					// проверяет правильность окончания заголовков
					case 5:
						if (@char == LF)
						{
							_Frame.Handl = 0;
							header.SetReq ();
							_Frame.GetHead = true;
						}
						else
							throw new HTTPException( "Отсутствует символ [LF]", HTTPCode._400_ );
						
						// Закрыть соединение
						if (header.Connection == "close")
							header.Close = true;
						
						// длинна тела запроса
						if (header.ContentLength > 0)
						{
							_Frame.Handl = 1;
							_Frame.bleng = header.ContentLength;
							header._Body = new byte[header.ContentLength];
							
						}
						// данные будут приходить по кускам
						if ( !string.IsNullOrEmpty(header.TransferEncoding) )
						{
							_Frame.Handl = 2;
							if (_Frame.bleng  >  0)
								throw new HTTPException( "Длинна тела задана не явно", HTTPCode._400_ );
							
						}
						break;
				}
					PointR++;
					_Frame.hleng++;
				if (_Frame.GetHead && header.IsReq )
					return read;
			}
            read = -1;
			return read;
		}
		public static void ParsePath(string strdata, IHeader header)
		{
			int index = strdata.LastIndexOf('.');
			if (index == -1)
				header.File = "html";
			else
				header.File = strdata.Substring(index + 1);
		}
	}
}
