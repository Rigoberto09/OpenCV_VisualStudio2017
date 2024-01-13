﻿using System;
using System.IO;
using OpenCvSharp;

namespace FaceDetection
{
    class Program
    {
        static void Main(string[] args)
        {
            string direccionVideo = "";
            do
            {
                Console.WriteLine("Ingrese la dirección del video con el nombre:");
                Console.WriteLine("ejemplo: C:/Users/rborjas/OneDrive/Escritorio/Videos/video.mp4");
                direccionVideo = Console.ReadLine();
                Console.WriteLine("\n");

                if (!string.IsNullOrEmpty(direccionVideo))
                {
                    var verificar = new VideoCapture(direccionVideo);
                    if (!verificar.IsOpened())
                    {
                        Console.WriteLine("No se pudo abrir el video. Inténtelo nuevamente.");
                        direccionVideo = "";
                    }
                }

            } while (string.IsNullOrEmpty(direccionVideo));

            string videoPath = direccionVideo;


            //string direccionVideo = "C:/Users/rborjas/OneDrive/Escritorio/Videos/v1.mp4";

            string framesPath = Path.Combine(Path.GetDirectoryName(videoPath), "img");
            
            int faceCount = 0;
            int eyeCount = 0;
            int minDuration = 10;

            string currentDirectory = Directory.GetCurrentDirectory();
            //string faceCascadePath = Path.Combine(currentDirectory, "haarcascade_frontalface_default.xml");
            //string eyeCascadePath = Path.Combine(currentDirectory, "haarcascade_eye.xml");

            ////en caso que no puededa detectar los archivos requeriso se ingrese la direccion completa tal y como aparece aqui
            string faceCascadePath = "C:/Users/rborjas/source/repos/OpenCV/OpenCV/haarcascade_frontalface_default.xml";
            string eyeCascadePath = "C:/Users/rborjas/source/repos/OpenCV/OpenCV/haarcascade_eye.xml";


            var faceCascade = new CascadeClassifier(faceCascadePath);
            var eyeCascade = new CascadeClassifier(eyeCascadePath);

            //Abrir el video para fotogramas
            using (var captureFotoGramas = new VideoCapture(videoPath))
            {

                // Verifica si el video se abrió correctamente


                if (!captureFotoGramas.IsOpened())
                {
                    Console.WriteLine("No se pudo abrir el video.");
                    Console.ReadLine();
                    return;
                }

                // Calculos de los fotogramas por segundo
                // Obtiene la tasa de frames por segundo (fps) y la duración total del video
                double fps = captureFotoGramas.Fps;
                int totalFrames = (int)captureFotoGramas.FrameCount;
                double totalDurationSeconds = totalFrames / fps;

                // Si la duración total es menor que min_duration, usa la duración total
                double durationToCapture = Math.Max(minDuration, totalDurationSeconds);
                int totalFramesToCapture = (int)(fps * durationToCapture);

                // Captura frames durante la duración especificada
                for (int frameNumber = 0; frameNumber < totalFramesToCapture; frameNumber++)
                {
                    using (var frame = new Mat())
                    {
                        if (!captureFotoGramas.Read(frame))
                        {
                            Console.WriteLine($"Finalizacion de fotogramas frame_{frameNumber}.png Precione enter para continuar con la Identifiacacion");
                            Console.ReadLine();
                            break;
                        }
                        Console.WriteLine($"Creando imagen {frameNumber}.");
                        //Console.ReadLine(); //habilitar para crear imgen por cada enter

                        // Guarda el frame como una imagen

                        //string frameFilename = Path.Combine(framesPath, $"frame_{frameNumber:0000}.png");
                        string frameFilename = Path.Combine(framesPath, $"frame_{frameNumber:0000}.png");

                        // Verificar si la carpeta de destino existe, si no, crearla
                        string folderPath = Path.GetDirectoryName(frameFilename);
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }

                        // Guardar el archivo
                        Cv2.ImWrite(frameFilename, frame);
                    }
                }
            }


            // Abrir el video para identificar partes
            using (var capture = new VideoCapture(videoPath))
            {
                // Verificar si el video se abrió correctamente
                if (!capture.IsOpened())
                {
                    Console.WriteLine("No se pudo abrir el video. presione enter para continuar");
                    Console.ReadLine();
                    return;
                }

                // Dimensiones del video
                capture.Set(3, 540);
                capture.Set(4, 380);

                while (true)
                {
                    using (var frame = new Mat())
                    {
                        if (!capture.Read(frame))
                        {
                            break;
                        }

                        var gray = new Mat();
                        Cv2.CvtColor(frame, gray, ColorConversionCodes.BGR2GRAY);

                        // Detección de caras
                        var faces = faceCascade.DetectMultiScale(gray, 1.1, 5, HaarDetectionTypes.DoCannyPruning, new Size(30, 30));

                        foreach (var (x, y, w, h) in faces)
                        {
                            var roiGray = gray[new Rect(x, y, w, h)];
                            var roiColor = frame[new Rect(x, y, w, h)];

                            // Detectar lo similar a un ojo. 
                            var eyes = eyeCascade.DetectMultiScale(roiGray);

                            foreach (var (ex, ey, ew, eh) in eyes)
                            {
                                Cv2.Rectangle(roiColor, new Point(ex, ey), new Point(ex + ew, ey + eh), Scalar.Red, 2);
                                eyeCount++;
                            }

                            Cv2.Rectangle(frame, new Point(x, y), new Point(x + w, y + h), Scalar.Green, 2);
                            faceCount++;
                        }

                        Cv2.ImShow("Identificacion de partes", frame);
                        Cv2.WaitKey(1);
                    }
                }
            }

            // Liberar la captura de video y cerrar todas las ventanas
            Cv2.DestroyAllWindows();

            // Mostrar el resumen
            Console.WriteLine($"Se detectaron {faceCount} caras y {eyeCount} ojos en el video.");
            Console.ReadLine();
        }
    }
}
