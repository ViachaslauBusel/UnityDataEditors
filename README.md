## Введение

Этот плагин для Unity предоставляет удобный инструмент для организации и управления данными игры. Он позволяет создавать и настраивать реестры игровых объектов, а также управлять ими через пользовательский интерфейс в редакторе Unity.

### Возможности
- Создание реестров для различных типов объектов.
- Управление данными через окно UI.
- Добавление, редактирование и удаление объектов в реестре.
- Поддержка фильтрации и сортировки данных.

## Установка

1. В окне **Project Manager** нажмите **Add package from git URL**.  
2. Вставьте ссылку в поле ввода: https://github.com/ViachaslauBusel/UnityDataEditors.git?path=/Packages/com.object.registry.editor
![Screenshot of my project](images/package_manager.png)

## Создание классов для хранения данных

## Создание реестра
```csharp
 [CreateAssetMenu(fileName = "ItemsRegistry", menuName = "Data/ItemsRegistry")]
 public class ItemsRegistry : DataObjectRegistry<ItemData>
 {
 }
```
 
## Создание объектов данных
Теперь создайте класс для хранения данных, реализующий интерфейс IDataObject:
```csharp
  public class ItemData : ScriptableObject, IDataObject
  {
      [SerializeField, HideInInspector]
      private int _id;
      [SerializeField]
      private string _name;
      [SerializeField]
      private Texture _icon;

      public int ID => _id;
      public string Name => _name;
      public Texture Preview => _icon;

      public void Initialize(int id)
      {
          _id = id;
      }
  }

## Использование

Создайте экземпляр реестра через контекстное меню Unity:
Right-click → Create → Data → ItemsRegistry.
Дважды кликните на созданный объект реестра, чтобы открыть окно управления:

![Screenshot of my project](images/window.png)
