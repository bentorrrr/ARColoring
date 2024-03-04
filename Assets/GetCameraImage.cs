using UnityEngine;
using UnityEngine.UI;
using Vuforia;
using Image = Vuforia.Image;

public class GetCameraImage : MonoBehaviour
{
    const PixelFormat PIXEL_FORMAT = PixelFormat.RGB888;
    const TextureFormat TEXTURE_FORMAT = TextureFormat.RGB24;

    public RawImage RawImage;

    Texture2D mTexture;
    bool mFormatRegistered;

    void Start()
    {
        // Register Vuforia Engine life-cycle callbacks:
        VuforiaApplication.Instance.OnVuforiaStarted += OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped += OnVuforiaStopped;
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.World.OnStateUpdated += OnVuforiaUpdated;
    }

    void OnDestroy()
    {
        // Unregister Vuforia Engine life-cycle callbacks:
        if (VuforiaBehaviour.Instance != null)
            VuforiaBehaviour.Instance.World.OnStateUpdated -= OnVuforiaUpdated;

        VuforiaApplication.Instance.OnVuforiaStarted -= OnVuforiaStarted;
        VuforiaApplication.Instance.OnVuforiaStopped -= OnVuforiaStopped;

        if (VuforiaApplication.Instance.IsRunning)
        {
            // If Vuforia Engine is still running, unregister the camera pixel format to avoid unnecessary overhead
            // Formats can only be registered and unregistered while Vuforia Engine is running
            UnregisterFormat();
        }

        if (mTexture != null)
            Destroy(mTexture);
    }

    /// 
    /// Called each time the Vuforia Engine is started
    /// 
    void OnVuforiaStarted()
    {
        mTexture = new Texture2D(0, 0, TEXTURE_FORMAT, false);
        // A format cannot be registered if Vuforia Engine is not running
        RegisterFormat();
    }

    /// 
    /// Called each time the Vuforia Engine is stopped
    /// 
    void OnVuforiaStopped()
    {
        // A format cannot be unregistered after OnVuforiaStopped
        UnregisterFormat();
        if (mTexture != null)
            Destroy(mTexture);
    }

    /// 
    /// Called each time the Vuforia Engine state is updated
    /// 
    void OnVuforiaUpdated()
    {
        var image = VuforiaBehaviour.Instance.CameraDevice.GetCameraImage(PIXEL_FORMAT);

        // There can be a delay of several frames until the camera image becomes available
        if (Image.IsNullOrEmpty(image))
            return;
        
        // Override the current texture by copying into it the camera image flipped on the Y axis
        // The texture is resized to match the camera image size
        image.CopyToTexture(mTexture, true);

        RawImage.texture = mTexture;
        RawImage.material.mainTexture = mTexture;
    }

    /// 
    /// Register the camera pixel format
    /// 
    void RegisterFormat()
    {
        // Vuforia Engine has started, now register camera image format
        var success = VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, true);
        if (success)
        {
            mFormatRegistered = true;
        }
        else
        {
            mFormatRegistered = false;
        }
    }

    /// 
    /// Unregister the camera pixel format
    /// 
    void UnregisterFormat()
    {
        VuforiaBehaviour.Instance.CameraDevice.SetFrameFormat(PIXEL_FORMAT, false);
        mFormatRegistered = false;
    }
}