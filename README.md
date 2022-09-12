# SimpleRecyclerCollection (v1.0.3)

Generic optimized ScrollRect for Unity that can handle a huge amount of data with using pooling objects. 

## How to use?
An example is included in the project so you can see how it works and how to implement it correctly.

1. Create a cell data class that will store all the information.
```csharp
public class MyCellData {
    public string Id;
    public string Title;
}
```

2. Create a cell view class that extends CellView and implement its abstract members. 
Don't forget about namespace ```SimpleRecyclerCollection```. Then just create a new GameObject and link ```MyMainCellView``` to this object and connect all the fields. After that, put it in resources.

```csharp
public class MyCellView: CellView<MyCellData> {
    // Important to set before initializing the collection.
    public override RectTransform RectTransform => m_RectTransform;
    [SerializeField] protected RectTransform m_RectTransform;

    [SerializeField] protected Text Title;

    // Methods

    private void OnValidate() {
        if (m_RectTransform == null)
            m_RectTransform = GetComponent<RectTransform>();
    }

    public override void OnContentUpdate(int index, MyCellData data) {
        Title.text = data.Title;
    }

    // Can use for animations.
    public override void OnPositionUpdate(Vector3 localPosition) {}
}
```

3. Create a collection class that extends RecyclerCollection. 
Don't forget about namespace ```SimpleRecyclerCollection```.
```csharp
public class MyCollection: RecyclerCollection<MyCellData, MyCellView> {}
```

4. Create a new GameObject in the scene in the canvas and add the ```MyCollection``` script to this object. 

5. Create a new GameObject inside the MyCollection and name it as Viewport. And like ScrollRect, it should have a component ```Mask``` or ```RectMask2D```, so add one of these. If you decide to add the ```Mask```, then don't forget to add the component ```Image```.

6. Create a new GameObject inside the Viewport and name it as Content, then add the component ```CollectionContent``` to it. It will add automatically the dependency script ```CollectionLayoutGroup```, through which you can change the properties ```auto tuples```, ```tuples```, ```expand```, ```align```, ```padding```, ```spacing``` for cells.

7. Back to the ```MyCollection```, and connect the ```Content```, ```Viewport``` and cell view prefab we put in resources. Cell view must be connected to its corresponding data class.

8. After that, we need to initialize ```MyCollection```.
```csharp
public class MyTest {
    [SerializedField] private MyCollection _collection;

    // Methods

    private void Start() {
        _collection.Initialize();
    }
}
```

## How to handle data?
```csharp
...
        // This is pseudocode example.
        private void HandleData() {
            // You can add/insert 1 item.
            _collection.Data.Add(new MyCellData());
            _collection.Data.Add(index, new MyCellData());

            // You can add/insert an array of items.
            _collection.Data.Add(MyCellData[]);
            _collection.Data.Insert(index, MyCellData[]);

            // You can add/insert a list of items.
            _collection.Data.Add(new List < MyCellData > ());
            _collection.Data.Insert(new List < MyCellData > ());

            // You can remove an item.
            _collection.Data.Remove(item);

            // You can remove an item at indedx.
            _collection.Data.RemoveAt(index);

            // You can replace all the data with new.
            _collection.Data.Replace(new MyCellData());
            _collection.Data.Replace(MyCellData[]);
            _collection.Data.Replace(new List < MyCellData > ());

            // You can clear all the data.
            _collection.Data.Clear();
        }

...
```

## Anything else?
```csharp
// This is pseudocode example.
// You can set normalized position.
_collection.NormalizedPosition = [0...1];

// You can snap to the spefic data.
_collection.SnapTo(index);
_collection.SnapTo(item);

// You can scroll to the specific data.
_collection.ScrollTo(index);
_collection.ScrollTo(item);

// You can directly change the properties of a CollectionLayoutGroup through the collection.
_collection.LayoutGroup.TuplesCount = value;
```

## License
[MIT](https://choosealicense.com/licenses/mit/)
