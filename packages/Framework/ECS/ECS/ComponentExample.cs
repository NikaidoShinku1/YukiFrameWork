using System.Collections.Generic;

namespace YukiFrameWork.ECS
{
    public delegate void ExportAction<T1>(T1 t1);
    public delegate void ExportAction<T1, T2>(T1 t1, T2 t2);
    public delegate void ExportAction<T1, T2, T3>(T1 t1, T2 t2, T3 t3);
    public delegate void ExportAction<T1, T2, T3, T4>(T1 t1, T2 t2, T3 t3, T4 t4);
    public delegate void ExportAction<T1, T2, T3, T4, T5>(T1 t1, T2 t2, T3 t3, T4 t4, T5 t5);
    public static class ComponentExample
    {
        public static void ForEach<T>(this List<Entity> entities, ExportAction<T> exportAction)
        {
            foreach (var entity in entities)
            {
                var component = entity.GetComponent<T>();

                if (component == null) return;

                exportAction?.Invoke(component);
            }
        }

        public static void ForEach<T1, T2>(this List<Entity> entities, ExportAction<T1, T2> exportAction)
        {
            foreach (var entity in entities)
            {
                var component1 = entity.GetComponent<T1>();
                var component2 = entity.GetComponent<T2>();

                if (component1 == null || component2 == null) return;

                exportAction?.Invoke(component1, component2);
            }
        }

        public static void ForEach<T1, T2, T3>(this List<Entity> entities, ExportAction<T1, T2, T3> exportAction)
        {
            foreach (var entity in entities)
            {
                var component1 = entity.GetComponent<T1>();
                var component2 = entity.GetComponent<T2>();
                var component3 = entity.GetComponent<T3>();
                if (component1 == null || component2 == null || component3 == null) return;

                exportAction?.Invoke(component1, component2, component3);
            }
        }

        public static void ForEach<T1, T2, T3, T4>(this List<Entity> entities, ExportAction<T1, T2, T3, T4> exportAction)
        {
            foreach (var entity in entities)
            {
                var component1 = entity.GetComponent<T1>();
                var component2 = entity.GetComponent<T2>();
                var component3 = entity.GetComponent<T3>();
                var component4 = entity.GetComponent<T4>();
                if (component1 == null || component2 == null || component3 == null || component4 == null) return;

                exportAction?.Invoke(component1, component2, component3, component4);
            }
        }

        public static void ForEach<T1, T2, T3, T4, T5>(this List<Entity> entities, ExportAction<T1, T2, T3, T4, T5> exportAction)
        {
            foreach (var entity in entities)
            {
                var component1 = entity.GetComponent<T1>();
                var component2 = entity.GetComponent<T2>();
                var component3 = entity.GetComponent<T3>();
                var component4 = entity.GetComponent<T4>();
                var component5 = entity.GetComponent<T5>();
                if (component1 == null || component2 == null || component3 == null || component4 == null || component5 == null) return;

                exportAction?.Invoke(component1, component2, component3, component4,component5);
            }
        }

        public static void ForEach(this Entity entity, ExportAction<Entity> exportAction)
        {
            exportAction?.Invoke(entity);
        }
    }
}


