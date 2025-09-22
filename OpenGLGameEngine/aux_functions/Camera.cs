using OpenTK.Mathematics;
using OpenTK.Windowing.GraphicsLibraryFramework;
using System.Drawing;
using Vector2 = OpenTK.Mathematics.Vector2;
using Vector3 = OpenTK.Mathematics.Vector3;

namespace OpenGLGameEngine.aux_functions
{
    public class Camera
    {
        private static Vector3 up = Vector3.UnitY; // CONST

        // scalars
        float speed;
        float sensitivity;

        float pitch;
        float yaw;
        float roll;

        float zNear;
        float zFar;
        float fov;


        // vectors
        Vector2 lastPos;
        Vector3 position; 
        Vector3 front;

        // matrices
        Matrix4 view;
        Matrix4 projection;
        


        // Main constructor
        public Camera(Vector3 positionOfCamera, Vector3 targetDirection, float speedCamera, float sensitivityCamera, float aspectRatio, float zNear = 0.1f, float zFar= 100.0f, float fov = 45.0f)
        {
            position = positionOfCamera;
            front = targetDirection;
            speed = speedCamera;
            sensitivity = sensitivityCamera;

            this.zFar = zFar;
            this.fov = fov;
            this.zNear = zNear;

            view = Matrix4.LookAt(position, position + front, up);
            projection = Matrix4.CreatePerspectiveFieldOfView(MathHelper.DegreesToRadians(fov), aspectRatio, zNear, zFar); // note that near/far clipping might need to be changed for render distance
            lastPos = new Vector2(0, 0);
        }

        // Position Controls
        public void useCamera(KeyboardState input, MouseState mouse, float deltaTime)
        {
            translation(input, deltaTime);
            rotation(mouse, deltaTime);

            view = Matrix4.LookAt(position, position + front, up);
        }

        public void updateDeltaXYMouse(MouseState mouse)
        {
            this.lastPos.X = mouse.X;
            this.lastPos.Y = mouse.Y;
        }
        private void rotation(MouseState mouse, float deltaTime)
        {
            float deltaX = mouse.X - lastPos.X;
            float deltaY = mouse.Y - lastPos.Y;
            lastPos = new Vector2(mouse.X, mouse.Y);

            //update 
            yaw += deltaX * sensitivity;
            pitch -= deltaY * sensitivity;

            // gimbal locking bitch
            if (pitch > 89.0f)
            {
                pitch = 89.0f;
            }
            else if (pitch < -89.0f)
            {
                pitch = -89.0f;
            }

            //update front vector
            front.X = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Cos(MathHelper.DegreesToRadians(yaw));
            front.Y = (float)Math.Sin(MathHelper.DegreesToRadians(pitch));
            front.Z = (float)Math.Cos(MathHelper.DegreesToRadians(pitch)) * (float)Math.Sin(MathHelper.DegreesToRadians(yaw));

            front = Vector3.Normalize(front);
        }

        private void translation(KeyboardState input, float deltaTime)
        {
            Vector3 direction = Vector3.Zero;

            if (input.IsKeyDown(Keys.W)) direction += front;       // Forward
            if (input.IsKeyDown(Keys.S)) direction -= front;       // Backward
            if (input.IsKeyDown(Keys.A)) direction -= Vector3.Normalize(Vector3.Cross(front, up)); // Left
            if (input.IsKeyDown(Keys.D)) direction += Vector3.Normalize(Vector3.Cross(front, up)); // Right
            if (input.IsKeyDown(Keys.Space)) direction += up;     // Up
            if (input.IsKeyDown(Keys.LeftShift)) direction -= up; // Down

            if (direction != Vector3.Zero)
            {
                position += Vector3.Normalize(direction) * speed * deltaTime;
            }
        }

        public Matrix4 getView()
        {
            return view;
        }

        public Matrix4 getProjection() {  return projection; }

        public Vector3 getViewPos()
        {
            return this.position;
        }
        public Vector3 getViewDirection()
        {
            return this.front;
        }

        public float getZNear() { return zNear; }
        public float getZFar() { return zFar; }


    }
}
