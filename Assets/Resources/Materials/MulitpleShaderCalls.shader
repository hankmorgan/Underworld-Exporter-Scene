Shader "MulitpleShaderCalls"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
	}
	SubShader
	{
	UsePass "COLORREPLACEMENT/COLORREPLACEMENTPASS"
	}
	
	Fallback "Legacy Shaders/VertexLit"
}
