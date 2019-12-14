﻿using Unity.Collections;
using Unity.Mathematics;
using Unity.Entities;
using UnityEngine;
using Unity.Jobs;

namespace Antypodish.ECS.Octree.Examples
{ 

    [DisableAutoCreation]
    class OctreeExample_GetCollidingBoundsInstancesSystem_Bounds2Octree : JobComponentSystem
    {
        
        EndInitializationEntityCommandBufferSystem eiecb ;
              
        protected override void OnCreate ( )
        {   

            // Test bounds
            // Many bounds, to octree pair
            // Where each bounds entty has one octree entity target.


            // Toggle manually only one example systems at the time
            // if ( !( OctreeExample_Selector.selector == Selector.GetCollidingBoundsInstancesSystem_Bounds2Octree ) ) return ; // Early exit

            
            Debug.Log ( "Start Test Get Colliding Bounds Instances System" ) ;


            // ***** Initialize Octree ***** //

            // Create new octree
            // See arguments details (names) of _CreateNewOctree and coresponding octree readme file.
            
            Entity newOctreeEntity = EntityManager.CreateEntity ( AddNewOctreeSystem.octreeArchetype ) ;

            eiecb = World.GetOrCreateSystem <EndInitializationEntityCommandBufferSystem> () ;
            EntityCommandBuffer ecb = eiecb.CreateCommandBuffer () ;

            AddNewOctreeSystem._CreateNewOctree ( ref ecb, newOctreeEntity, 8, float3.zero, 1, 1 ) ;

            // Assign target bounds entity, to octree entity
            Entity octreeEntity = newOctreeEntity ;    



            // ***** Example Components To Add / Remove Instance ***** //
            
            // Example of adding and removing some instanceses, hence entity blocks.


            // Add

            // ComponentDataFromEntity <Bootstrap.RenderMeshTypesData> componentDataFromEntity = GetComponentDataFromEntity <Bootstrap.RenderMeshTypesData> ( true ) ;
            // RenderMeshTypesData renderMeshTypes = EntityManager.GetComponentData <RenderMeshTypesData> ( Bootstrap.renderMeshTypesEntity ) ;
            // Bootstrap.EntitiesPrefabsData entitiesPrefabs = EntityManager.GetComponentData <Bootstrap.EntitiesPrefabsData> ( Bootstrap.entitiesPrefabsEntity ) ;

            int i_instances2AddCount                      = OctreeExample_Selector.i_generateInstanceInOctreeCount ; // Example of x octrees instances. // 1000
            NativeArray <Entity> na_instanceEntities      = Common._CreateInstencesArray ( EntityManager, i_instances2AddCount ) ;
                            
            // Request to add n instances.
            // User is responsible to ensure, that instances IDs are unique in the octrtree.
            
            ecb.AddComponent <AddInstanceTag> ( octreeEntity ) ; // Once system executed and instances were added, tag component will be deleted.   
            // EntityManager.AddBuffer <AddInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were added, buffer will be deleted.        
            BufferFromEntity <AddInstanceBufferElement> addInstanceBufferElement = GetBufferFromEntity <AddInstanceBufferElement> () ;

            Common._RequesAddInstances ( ref ecb, octreeEntity, addInstanceBufferElement, ref na_instanceEntities, i_instances2AddCount ) ;



            // Remove
                
            ecb.AddComponent <RemoveInstanceTag> ( octreeEntity ) ; // Once system executed and instances were removed, tag component will be deleted.
            // EntityManager.AddBuffer <RemoveInstanceBufferElement> ( octreeEntity ) ; // Once system executed and instances were removed, component will be deleted.
            BufferFromEntity <RemoveInstanceBufferElement> removeInstanceBufferElement = GetBufferFromEntity <RemoveInstanceBufferElement> () ;
                
            // Request to remove some instances
            // Se inside method, for details
            int i_instances2RemoveCount = OctreeExample_Selector.i_deleteInstanceInOctreeCount ; // Example of x octrees instances / entities to delete. // 53
            Common._RequestRemoveInstances ( ref ecb, octreeEntity, removeInstanceBufferElement, ref na_instanceEntities, i_instances2RemoveCount ) ;
                
                
            // Ensure example array is disposed.
            na_instanceEntities.Dispose () ;





            // ***** Example Bounds Components For Collision Checks ***** //

            Debug.Log ( "Octree: create dummy (for visualization only) boundary box, to test for collision." ) ;
            float3 f3_blockCenter = new float3 ( 10, 2, 3 ) ;
            // Only test
            Entity boundsEntity = EntityManager.Instantiate ( PrefabsSpawner_FromEntity.spawnerEntitiesPrefabs.boundingBoxEntity ) ;            
            Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ref ecb, boundsEntity, f3_blockCenter, new float3 ( 1, 1, 1 ) * 5, MeshType.BoundingBox ) ;
            // Blocks.PublicMethods._AddBlockRequestViaCustomBufferWithEntity ( ecb, EntityManager.CreateEntity ( ), f3_blockCenter, new float3 ( 1, 1, 1 ) * 5 ) ;

            // Create test bounds
            // Many bounds, to single or many octrees
            // Where each bounds has one octree entity target.
            for ( int i = 0; i < 10; i ++ ) 
            {

                Entity testEntity = ecb.CreateEntity ( BlocksArchetypes.blockArchetype ) ; // Check bounds collision with octree and return colliding instances.      
                
                ecb.AddComponent ( testEntity, new IsActiveTag () ) ; 
                ecb.AddComponent ( testEntity, new GetCollidingBoundsInstancesTag () ) ;                  
                // This may be overritten by, other system. Check corresponding collision check system.
                ecb.AddComponent ( testEntity, new BoundsData ()
                {
                    bounds = new Bounds () { center = float3.zero, size = new float3 ( 5, 5, 5 ) }
                } ) ; 
                // Check bounds collision with octree and return colliding instances.
                ecb.AddComponent ( testEntity, new OctreeEntityPair4CollisionData () 
                {
                    octree2CheckEntity = newOctreeEntity
                } ) ;
                ecb.AddComponent ( testEntity, new IsCollidingData () ) ; // Check bounds collision with octree and return colliding instances.
                ecb.AddBuffer <CollisionInstancesBufferElement> ( testEntity ) ;

            } // for
                
        }

        protected override JobHandle OnUpdate ( JobHandle inputDeps )
        {
            return inputDeps ;
        }

    }
}


