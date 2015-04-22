using System.Net.Sockets;
using System.Text;

namespace TwoWholeWorms.Lumbricus.Shared
{
	public class SocketState
	{
		public Socket workSocket = null;
		public const int BufferSize = 256;
		public byte[] buffer = new byte[BufferSize];
		public StringBuilder sb = new StringBuilder();
	}
}

