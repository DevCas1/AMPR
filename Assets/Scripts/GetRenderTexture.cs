using System;
using AMPR.ExtensionMethod;
using UnityEngine;

namespace AMPR
{
    //internal enum BlurStatus { SetBlur, Setting, Blurring, Removing, Done, None }

    public class GetRenderTexture : MonoBehaviour
    {
        //public Camera Camera;
        //public MeshRenderer Renderer;
        //public RenderTexture RenderTexture;
        
        //private bool _imageSet;
        //private float _blurFill;
        //private BlurStatus _blurStatus = BlurStatus.None;

        //private static readonly int BlurStrength = Shader.PropertyToID("_blurStrength");

        //private void Start()
        //{
        //    if (Camera == null)
        //        Camera = Camera.main;

        //    Camera.onPostRender += Test;

        //    FindObjectOfType<PlayerController>().Controls.Player.Fire.performed += context => OnBlurInput();
        //    Renderer.material.SetFloat(BlurStrength, 0);
        //    //Renderer.gameObject.SetActive(false);
        //}

        //private void OnRenderImage(RenderTexture src, RenderTexture dst)
        //{
        //    Debug.Log("Yeah, OnRenderImage does work");

        //    Renderer.material.mainTexture = src.ToTexture2D();
        //    return;

        //    if (_imageSet)
        //        return;

        //    if (_blurStatus != BlurStatus.Blurring)
        //        return;

        //    Renderer.gameObject.SetActive(true);
        //    Renderer.material.SetFloat(BlurStrength, 0);
        //    Renderer.material.mainTexture = src.ToTexture2D();
        //    _imageSet = true;
        //}

        //private void OnBlurInput()
        //{
        //    switch (_blurStatus)
        //    {
        //        case BlurStatus.SetBlur:

        //            break;
        //        case BlurStatus.Blurring:
        //            break;
        //        case BlurStatus.Removing:
        //            break;
        //        case BlurStatus.Done:
        //            break;
        //        case BlurStatus.None:
        //            break;
        //        default:
        //            throw new ArgumentOutOfRangeException();
        //    }
        //}

        //// Update is called once per frame
        //private void Update()
        //{
        //    if (_blurStatus == BlurStatus.SetBlur && !_imageSet)
        //    {
        //        SetImage();
        //        return;
        //    }

        //    //if (_blurStatus != BlurStatus.Blurring || _blurStatus != BlurStatus.Removing )
        //    //    return;

        //    //switch (_blurStatus)
        //    //{
        //    //    case BlurStatus.Done when _blurFill >= 1:
        //    //    case BlurStatus.None when _blurFill == 0:
        //    //    case BlurStatus.Blurring when !_imageSet:
        //    //        return;
        //    //}

        //    //_blurFill = Mathf.SmoothStep(0, 1, _blurFill + (_blurStatus == BlurStatus.Blurring ? Time.deltaTime : -Time.deltaTime));
        //    //Renderer.material.SetFloat(BlurStrength, _blurFill);

        //    //if (_blurFill >= 1)
        //    //{
        //    //    _blurFill = 1;
        //    //    _blurStatus = BlurStatus.Done;
        //    //    return;
        //    //}

        //    //if (_blurFill > 0)
        //    //    return;

        //    //_blurFill = 0;
        //    //_blurStatus = BlurStatus.Done;
        //    //_imageSet = false;
        //    //Renderer.gameObject.SetActive(false);
        //}

        //private void Test(Camera cam) => Debug.Log("Test");

        //private void SetImage()
        //{
        //    _blurStatus = BlurStatus.Setting;
        //    Camera.targetTexture = RenderTexture;
        //}
    }
}