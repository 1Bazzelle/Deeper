using UnityEngine;
using UnityEngine.UIElements;

public interface IScreen
{
    public void Initialize(VisualTreeAsset tree, VisualElement root);
    public void Update();
}
