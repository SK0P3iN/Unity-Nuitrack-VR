#region Description

// The script performs a direct translation of the skeleton using only the position data of the joints.
// Objects in the skeleton will be created when the scene starts.

#endregion


using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI;
using System;

[AddComponentMenu("Nuitrack/Example/TranslationAvatar")]
public class CodigoPesoMorto : MonoBehaviour
{
    string message = "";

    public nuitrack.JointType[] typeJoint;
    GameObject[] CreatedJoint;
    public GameObject PrefabJoint;

    private const int nRepeticoes= 10; //repetiçoes a fazer cada perna
    private const float tempoDescanso = 15f; //tempo de descanso entre as series
    private const int totalSeries = 3; //numero total de series a fazer
    private int serie = 1;

    private bool trocouPerna = false;

    const float scalK = 0.01f;
    private bool isPosicaoInicialGuardada = false, isAgachado = false;
    Vector3[] coordenadasJoints = new Vector3[9];
    public Text timerMensagem, contadorRepeticoes;
    private float timerComecar = 5f;
    
    Vector3[] posInicial_Y = new Vector3[9];
    


    private int repeticoes = nRepeticoes;
    private float timerDescanso = tempoDescanso;
    void Start()
    {
        CreatedJoint = new GameObject[typeJoint.Length];
        for (int q = 0; q < typeJoint.Length; q++)
        {
            CreatedJoint[q] = Instantiate(PrefabJoint);
            CreatedJoint[q].transform.SetParent(transform);
        }
        message = "Skeleton created";

        timerMensagem.text = "Coloque-se na posição inicial!";

    }

    void Update()
    {
        if (CurrentUserTracker.CurrentUser != 0)
        {
            nuitrack.Skeleton skeleton = CurrentUserTracker.CurrentSkeleton;
            message = "";
            
            for (int q = 0; q < typeJoint.Length; q++)
            {
                nuitrack.Joint joint = skeleton.GetJoint(typeJoint[q]);
                Vector3 newPosition = 0.001f * joint.ToVector3();
                CreatedJoint[q].transform.localPosition = newPosition;
            }

            contadorRepeticoes.text = repeticoes.ToString(); //atualiza mensagem dos agachamentos

            coordenadasJoints[0] = scalK * skeleton.GetJoint(nuitrack.JointType.Head).ToVector3(); //Cabeça
            coordenadasJoints[1] = scalK * skeleton.GetJoint(nuitrack.JointType.Neck).ToVector3(); //Pescoço
            coordenadasJoints[2] = scalK * skeleton.GetJoint(nuitrack.JointType.RightShoulder).ToVector3(); //ombro direito
            coordenadasJoints[3] = scalK * skeleton.GetJoint(nuitrack.JointType.LeftShoulder).ToVector3(); //ombro esquerdo
            coordenadasJoints[4] = scalK * skeleton.GetJoint(nuitrack.JointType.Torso).ToVector3(); //torso

            coordenadasJoints[5] = scalK * skeleton.GetJoint(nuitrack.JointType.RightElbow).ToVector3(); //braco direito
            coordenadasJoints[6] = scalK * skeleton.GetJoint(nuitrack.JointType.LeftElbow).ToVector3(); //braco esquerdo
            coordenadasJoints[7] = scalK * skeleton.GetJoint(nuitrack.JointType.RightKnee).ToVector3(); //joelho direito
            coordenadasJoints[8] = scalK * skeleton.GetJoint(nuitrack.JointType.LeftKnee).ToVector3(); //joelho esquerdo

            
            print("CABECA: " + coordenadasJoints[0]);
            print("PESCOCO: " + coordenadasJoints[1]);
            print("OMBRO DIREITO: " + coordenadasJoints[2]);
            print("OMBRO ESQUERDO: " + coordenadasJoints[3]);
            print("TORSO: " + coordenadasJoints[4]);
            print("BRACO DIREITO: " + coordenadasJoints[5]);
            
            print("BRACO ESQUERDO: " + coordenadasJoints[6]);
            print("JOELHO DIREITO: " + coordenadasJoints[7]);
            print("JOELHO ESQUERDO: " + coordenadasJoints[8]);


            if(!isPosicaoInicialGuardada){ //inicia a contagem decrescente para guardar a posição inicial e iniciar o exercício
                timerComecar -= Time.deltaTime;;
                timerMensagem.text = timerComecar.ToString();
                
                if(timerComecar <0)
                    GuardaPosicaoInicial(coordenadasJoints);

                return;
            }
            
            
            if(repeticoes > 0)
            {
                VerificaRepeticao(coordenadasJoints);

                if(isAgachado){
                    ContaAgachamento(coordenadasJoints);
                }
            }
            else{

                if(trocouPerna)
                    DescansaProximaSerie();
                else{ //troca de perna
                    repeticoes = nRepeticoes;
                    trocouPerna = true;
                    timerMensagem.text = "Vamos lá! Lentanta a perna direita";
                }
                
            }

        }
        else
        {
            message = "Skeleton not found!";
        }
    }

    private void DescansaProximaSerie()
    {
        if(serie == totalSeries)
            contadorRepeticoes.text = "Acabou o exercício!";
        else {
            contadorRepeticoes.text = "Terminas-te a " + serie + " série\n Volta à levantar a perna esquerda!";
            
            timerDescanso -= Time.deltaTime;
            timerMensagem.text = "Descanso: " + timerDescanso.ToString();

            if(timerDescanso < 0){
                timerMensagem.text = "Vamos lá! Lentanta a perna esquerda";
                repeticoes = nRepeticoes;
                trocouPerna = false;
                serie++;
                timerDescanso = tempoDescanso;
            }
            
        }
    }

    private void ContaAgachamento(Vector3[] coordenadasJoints)
    {
        //calcula se voltou as unidades iniciais
        for(int i=0; i<4;i++){
            float posDeltaY = posInicial_Y[i].y - coordenadasJoints[i].y;
            float posDeltaZ = posInicial_Y[i].z - coordenadasJoints[i].z;
            
            if(posDeltaY > 1.0f && posDeltaZ > 1.0f)
                return;
        }

        /*if(trocouPerna)
        {
            if(posInicial_Y[8].y - coordenadasJoints[8].y > 1.0f && posInicial_Y[8].z - coordenadasJoints[8].z > 1.0f) return; //mexeu a perna esquerda nao conta
        }
        else{
            if(posInicial_Y[7].y - coordenadasJoints[7].y > 1.0f && posInicial_Y[7].z - coordenadasJoints[7].z > 1.0f) return; //mexeu a perna direita nao conta
        }*/
        
        isAgachado = false;
        repeticoes--;
    }

    private void VerificaRepeticao(Vector3[] coordenadasJoints)
    {
        //verifica se desceu as unidades necessárias no y e no z
        /*for(int i=0; i<6;i++){
            float posDeltaY = posInicial_Y[i].y - coordenadasJoints[i].y;
            float posDeltaZ = posInicial_Y[i].z - coordenadasJoints[i].z;
            
            if(posDeltaY < 5.0f && posDeltaZ < 5.0f)
                return;
        }

        if(trocouPerna)
        {
            if(posInicial_Y[8].y - coordenadasJoints[8].y > 1.0f && posInicial_Y[8].z - coordenadasJoints[8].z > 1.0f) return; //mexeu a perna esquerda nao conta
        }
        else{
            if(posInicial_Y[7].y - coordenadasJoints[7].y > 1.0f && posInicial_Y[7].z - coordenadasJoints[7].z > 1.0f) return; //mexeu a perna direita nao conta
        }*/

        float deltaCabeca_Y = posInicial_Y[0].y - coordenadasJoints[0].y;
        float deltaCabeca_Z = posInicial_Y[0].z - coordenadasJoints[0].z;

        float deltaPescoco_Y = posInicial_Y[1].y - coordenadasJoints[1].y;
        float deltaPescoco_Z = posInicial_Y[1].z - coordenadasJoints[1].z;

        float deltaOmbDireito_Y = posInicial_Y[2].y - coordenadasJoints[2].y;
        float deltaOmbDireito_Z = posInicial_Y[2].z - coordenadasJoints[2].z;

        float deltaOmbEsquerdo_Y = posInicial_Y[3].y - coordenadasJoints[3].y;
        float deltaOmbEsquerdo_Z = posInicial_Y[3].z - coordenadasJoints[3].z;

        float deltaTorso_Y = posInicial_Y[4].y - coordenadasJoints[4].y;
        float deltaTorso_Z = posInicial_Y[4].z - coordenadasJoints[4].z;

        float deltabracoDireito_Y = posInicial_Y[5].y - coordenadasJoints[5].y;

        float deltabracoEsquerdo_Y = posInicial_Y[6].y - coordenadasJoints[6].y;

        /*print("DELTA_CAB: " + deltaCabeca_Y.ToString() + ", " + deltaCabeca_Z.ToString());
        print("DELTA_PESC: " + deltaPescoco_Y.ToString() + ", " + deltaPescoco_Z.ToString());
        print("DELTA_ombroD: " + deltaOmbDireito_Y.ToString() + ", " + deltaOmbDireito_Z.ToString());
        print("DELTA_ombroE: " + deltaOmbEsquerdo_Y.ToString() + ", " + deltaOmbEsquerdo_Z.ToString());
        print("DELTA_TORSO: " + deltaTorso_Y.ToString() + ", " + deltaTorso_Z.ToString());

        print("DELTA_bacoD: " + deltabracoDireito_Y.ToString());
        print("DELTA_bracoE: " + deltabracoEsquerdo_Y.ToString());*/

        if(deltaCabeca_Y < 3.5f || deltaPescoco_Y < 2.5f || deltaOmbDireito_Y < 2.0f || deltaOmbEsquerdo_Y < 2.0f || deltaTorso_Y <2.0f)
            return;

        if(deltaCabeca_Z < 4.5f || deltaPescoco_Z < 3.5f || deltaOmbDireito_Z < 3.0f || deltaOmbEsquerdo_Z < 3.0f || deltaTorso_Z <1.5f)
            return;

        if(trocouPerna){
            if(deltabracoDireito_Y > deltabracoEsquerdo_Y){
                    print("braco errado, baixo o direito");
                    return;
            }
                

        }else{
            if(deltabracoEsquerdo_Y > deltabracoDireito_Y){
                print("braco errado, baixa o esquerdo");
                return;
            }
                
                
        }


        isAgachado = true;
    }

    private void GuardaPosicaoInicial(Vector3[] coordenadasJoints)
    {
        for(int i=0; i<7;i++){ // cabeça; pescoço; ombro direito; ombro esquerdo
            posInicial_Y[i] = coordenadasJoints[i];
        }

        timerMensagem.text = "Vamos lá! Levanta a perna Esquerda";
        isPosicaoInicialGuardada = true;
    }



    // Display the message on the screen
    void OnGUI()
    {
        GUI.color = Color.red;
        GUI.skin.label.fontSize = 50;
        GUILayout.Label(message);
    }
}