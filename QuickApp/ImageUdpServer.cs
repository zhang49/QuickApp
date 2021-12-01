using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Threading;
using System.Drawing;

namespace QuickApp
{
    public class ImageUdpServer
    {
        public delegate void OnRecv(byte[] data);

        private UdpClient mUdp = null;
        private IPEndPoint mLocalEp = null;
        private IPEndPoint mDefaultRemoteEp = null;
        private bool mIsUdpcRecvStart = false;
        private Thread thrRecv;
        private OnRecv mOnRecv;
        private Size mImageSize;
        private byte[] mLastImageData;

        public void SetDefaultRemoteEp(string ip, int port)
        {
            mDefaultRemoteEp = new IPEndPoint(IPAddress.Parse(ip), port);
        }

        public void Start(OnRecv onRecv)
        {
            mOnRecv = onRecv;
            if (!mIsUdpcRecvStart) // 未监听的情况，开始监听
            {
                mLocalEp = new IPEndPoint(IPAddress.Parse("127.0.0.1"), 18088); // 本机IP和监听端口号
                mUdp = new UdpClient(mLocalEp);
                thrRecv = new Thread(ReceiveMessageProcess);
                thrRecv.Start();
                mIsUdpcRecvStart = true;
            }
        }

        public void Stop()
        {
            if (mIsUdpcRecvStart)
            {
                thrRecv.Abort();
                mUdp.Close();
                mIsUdpcRecvStart = false;
            }
        }

        private void ReceiveMessageProcess(object obj)
        {
            while (mIsUdpcRecvStart)
            {
                try
                {
                    byte[] recvBytes = mUdp.Receive(ref mLocalEp);
                    string message = Encoding.UTF8.GetString(recvBytes, 0, recvBytes.Length);
                    mOnRecv?.Invoke(recvBytes);
                }
                catch (Exception ex)
                {
                    break;
                }
            }
        }

        private void SendMessage(byte[] data, string ip, int port)
        {
            try
            {
                IPEndPoint remoteIpep = new IPEndPoint(IPAddress.Parse(ip), port); // 发送到的IP地址和端口号
                mUdp.Send(data, data.Length, remoteIpep);
            }
            catch { }
        }

        private void SendMessage(byte[] data, IPEndPoint ep)
        {
            try
            {
                mUdp.Send(data, data.Length, ep);
            }
            catch { }
        }

        private void SendMessage(byte[] data)
        {
            try
            {
                mUdp.Send(data, data.Length, mDefaultRemoteEp);
            }
            catch { }
        }
        /// <summary>
        /// 发送前调用，设置第一帧图片
        /// </summary>
        /// <param name="imageData"></param>
        private void PrepareSend(byte[] imageData, int w, int h)
        {
            //图片步长4
            mImageSize = new Size(w, h);
            mLastImageData = imageData;
        }
        /// <summary>
        /// 传入图像数据
        /// </summary>
        /// <param name="imageData"></param>
        private void SendImageData(byte[] imageData)
        {

        }


        private void SendImageData(byte[][] sections)
        {
            for (int i = 0; i < sections.Length; i++)
            {
                byte[] tmpBytes = new byte[sections[i].Length + 2];

                tmpBytes[0] = 0;
                tmpBytes[1] = 0;
                sections[i].CopyTo(tmpBytes, 2);
                SendMessage(tmpBytes);
            }
        }
    }
}
