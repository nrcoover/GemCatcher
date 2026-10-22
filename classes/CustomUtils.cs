using Godot;

public static class CustomUtils {
  public static void ActivateArea2D(Area2D area)
  {
    area.SetDeferred(Area2D.PropertyName.Monitoring, true);
    area.SetDeferred(Area2D.PropertyName.Monitorable, true);
  }

	public static void DeactivateArea2D(Area2D area)
  {
    area.SetDeferred(Area2D.PropertyName.Monitoring, false);
    area.SetDeferred(Area2D.PropertyName.Monitorable, false);
  }
}
