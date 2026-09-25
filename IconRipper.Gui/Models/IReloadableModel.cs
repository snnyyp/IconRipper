using System.Threading.Tasks;

namespace IconRipper.Gui.Models;

public interface IReloadableModel
{
    public Task Load();

    public Task Unload();
}