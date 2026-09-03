using Enjune.Physic;

namespace EcsPhysicEngine
{
    public class EcsPhysicsEngine : IPhysicsEngine
    {
        public IPhysicsWorld CreateWorld()
        {
            throw new NotImplementedException();
        }



        #region TheAbyss
        // fuck this stupid useless chungis shitfuck method that exists purely to annoy me by its presence while serving no purpose whatsoever
        public void Dispose()
        {
            //throw new NillKiggersException();
            return;
        }
        #endregion
    }
}
