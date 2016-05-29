﻿using System;
using System.IO;

namespace ChatConnect.Tcp.Protocol
{
	class StreamS : Stream
	{
		public long Count
		{
			get
			{
				return _len;
			}
		}
		public bool Empty
		{
			get
			{
				return (_p_r == _p_w);
			}
		}
		public long Clear
		{
			get
			{
				if (_p_w < _p_r)
					return (_p_r - _p_w);
				else
					return (_len - _p_w) + _p_r;
			}
		}		
		long _p_w;
		public long PointR
		{
			get
			{
				return _p_r;
			}
			protected set
			{
				if (value < _len)
					_p_r = value;
				else
					_p_r = 0;
			}
		}
		long _p_r;
		public long PointW
		{
			get
			{
				return _p_w;
			}
			protected set
			{
				if (value < _len)
					_p_w = value;
				else
					_p_w = 0;
				if (_p_w == _p_r)
					throw new IOException("Переаолнение буффера");
			}
		}
		public byte[] Buffer
		{
			get
			{
				return _buffer;
			}
		}
		public override long Length
		{
			get
			{
				if (_p_w < _p_r)
					return (_len - _p_r) + _p_w;					
				else
					return (_p_w - _p_r);
			}
		}
		public override bool CanRead
		{
			get
			{
				return true;
			}
		}
		public override bool CanSeek
		{
			get
			{
				return true;
			}
		}
		public override bool CanWrite
		{
			get
			{
				return true;
			}
		}
		public override long Position
		{
			get
			{
				return 0;
			}

			set
			{
				Seek(value, SeekOrigin.Begin);
			}
		}
		protected long _len;
		protected byte[] _buffer;


		public virtual int ReadHead()
		{
			throw new NotImplementedException();
		}
		public virtual int ReadBody()
		{
			throw new NotImplementedException();
		}
		public override void Flush()
		{
			throw new NotImplementedException();
		}
		public override int ReadByte()
		{
			if (Empty)
				return -1;
			return _buffer[_p_r++];
		}				
		public override void SetLength(long value)
		{
			if (value > Clear)
				throw new IOException();
			if (PointW + value < _len)
				PointW = PointW + value;
			else
				PointW = value - (Count - PointW);
		}
		public override long Seek(long offset, SeekOrigin origin)
		{
			if (offset > 0)
			{
				if (offset > Length)
					throw new IOException();
				if (offset + PointR < Count)
					PointR = PointR + offset;
				else
					PointR = offset - (Count - PointR);
				return offset;
			}
			else
			{
				if (offset > Clear)
					throw new IOException();
				if (PointR - offset > 0)
					PointR = PointR - offset;
				else
					PointR = Count - (offset - PointR);
				return offset * -1;
			}
		}
		unsafe public override int Read(byte[] buffer, int pos, int len)
		{
			int i;
			if (Empty)
				return -1;
			fixed(byte* source = buffer, target = _buffer)
			{
				
				byte* ps = source + pos;
				for (  i = 0; i < len; i++  )
				{
					byte* pt = target + PointR;

					*ps = *pt;
					ps++;
					PointR++;
					if (Empty)
						return i;
				}
			}
			return i;
		}
		unsafe public override void Write(byte[] buffer, int pos, int len)
		{
			int i;
			fixed(byte* source = _buffer, target = buffer)
			{
				byte* pt = target + pos;
				for (  i = 0; i < len; i++  )
				{
					byte* ps = source + PointW;

					*ps = *pt;					
					pt++;
					PointW++;
					if (!Empty)
						throw new IOException();
				}
			}
		}
	}
}