using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Buffers;
using System.Net.Sockets;
using System.Net.Sockets.Kcp;
using System.Threading;
using System.Threading.Tasks;

namespace QuickApp
{
    public class KcpServer
    {
        private static string ShowThread
        {
            get
            {
                return $"  ThreadID[{Thread.CurrentThread.ManagedThreadId}]";
            }
        }
        public class Handle : IKcpCallback
        {
            //public void Output(ReadOnlySpan<byte> buffer)
            //{
            //    var frag = new byte[buffer.Length];
            //    buffer.CopyTo(frag);
            //    Out(frag);
            //}

            public Action<Memory<byte>> Out;
            public Action<byte[]> Recv;
            public void Receive(byte[] buffer)
            {
                Recv(buffer);
            }

            public IMemoryOwner<byte> RentBuffer(int lenght)
            {
                return null;
            }

            public void Output(IMemoryOwner<byte> buffer, int avalidLength)
            {
                using (buffer)
                {
                    Out(buffer.Memory.Slice(0, avalidLength));
                }
            }
        }


        public static void Init()
        {
        }
        public class CRentable : IRentable
        {
            public IMemoryOwner<byte> RentBuffer(int length)
            {
                return new CIMemoryOwner(new byte[length]);
            }
            public class CIMemoryOwner : IMemoryOwner<byte>
            {
                byte[] data;
                public CIMemoryOwner(byte[] vs)
                {
                    data = vs;
                }

                public Memory<byte> Memory => new Memory<byte>(data);

                public void Dispose()
                {

                }
            }

        }
        public static void StartDemo()
        {
            Console.WriteLine(ShowThread);
            Random random = new Random();

            var handle1 = new Handle();
            var handle2 = new Handle();

            const int conv = 123;
            //var kcp1 = new UnSafeSegManager.Kcp(conv, handle1);
            //var kcp2 = new UnSafeSegManager.Kcp(conv, handle2);

            //var kcp1 = new PoolSegManager.Kcp(conv, handle1, new CRentable());
            //var kcp2 = new PoolSegManager.Kcp(conv, handle2, new CRentable());
            var kcp1 = new PoolSegManager.Kcp(conv, handle1);
            var kcp2 = new PoolSegManager.Kcp(conv, handle2);


            kcp1.NoDelay(1, 10, 2, 1);//fast
            kcp1.WndSize(64, 64);
            kcp1.SetMtu(512);

            kcp2.NoDelay(1, 10, 2, 1);//fast
            kcp2.WndSize(64, 64);
            kcp2.SetMtu(512);
            string message = "";
            for (int i = 0; i < 10000; i++)
            {
                message += (char)(0x30 + i % 10);
            }
            for (int i = 0; i < 2; i++)
            {
                message += message;
            }
            var sendbyte = Encoding.ASCII.GetBytes(message);

            int buffercount = 0;
            handle1.Out += buffer =>
            {
                buffercount += buffer.Length ;
                int ret = kcp2.Input(buffer.Span);
                Console.WriteLine($"11------Thread[{Thread.CurrentThread.ManagedThreadId}],ret:{ret},count:{buffercount}");
                Thread.Sleep(50);
                //Task.Run(async () =>
                //{
                //    Console.WriteLine($"12------Thread[{Thread.CurrentThread.ManagedThreadId}]");
                //});
            };

            handle2.Out += buffer =>
            {
                Task.Run(() =>
                {
                    kcp1.Input(buffer.Span);
                });
            };
            int count = 0;

            handle1.Recv += buffer =>
            {
                var str = Encoding.ASCII.GetString(buffer);
                count++;
                if (message == str)
                {
                    Console.WriteLine($"kcp  echo----{count}");
                }
                var res = kcp1.Send(buffer);
                if (res != 0)
                {
                    Console.WriteLine($"kcp send error");
                }
            };

            int recvCount = 0;
            handle2.Recv += buffer =>
            {
                recvCount++;
                Console.WriteLine($"kcp2 recv----{recvCount}");
                var res = kcp2.Send(buffer);
                if (res != 0)
                {
                    Console.WriteLine($"kcp send error");
                }
            };

            Task.Run(async () =>
            {
                try
                {
                    int updateCount = 0;
                    while (true)
                    {
                        kcp1.Update(DateTime.UtcNow);

                        int len;
                        while ((len = kcp1.PeekSize()) > 0)
                        {
                            var buffer = new byte[len];
                            if (kcp1.Recv(buffer) >= 0)
                            {
                                handle1.Receive(buffer);
                            }
                        }

                        await Task.Delay(5);
                        updateCount++;
                        if (updateCount % 1000 == 0)
                        {
                            Console.WriteLine($"KCP1 ALIVE {updateCount}----{ShowThread}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }

            });

            Task.Run(async () =>
            {
                try
                {
                    int updateCount = 0;
                    while (true)
                    {
                        kcp2.Update(DateTime.UtcNow);

                        //var utcNow = DateTime.UtcNow;
                        //var res = kcp2.Check(utcNow);

                        int len;
                        do
                        {
                            var (buffer, avalidSzie) = kcp2.TryRecv();
                            len = avalidSzie;
                            if (buffer != null)
                            {
                                var temp = new byte[len];
                                buffer.Memory.Span.Slice(0, len).CopyTo(temp);
                                handle2.Receive(temp);
                            }
                        } while (len > 0);

                        await Task.Delay(5);
                        updateCount++;
                        if (updateCount % 1000 == 0)
                        {
                            Console.WriteLine($"KCP2 ALIVE {updateCount}----{ShowThread}");
                        }
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
            });

            int rr = kcp1.Send(sendbyte);

            while (true)
            {
                Thread.Sleep(1000);
                GC.Collect();
            }

            Console.ReadLine();
        }
    }
}
