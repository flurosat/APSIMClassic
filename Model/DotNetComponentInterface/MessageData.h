#pragma once

#include "Interfaces.h"
#include "..\DataTypes\DOTNETDataTypes.h"
#include <string>
#include <vcclr.h>
using namespace System::Runtime::InteropServices;
using namespace System::Reflection;
using namespace System::Collections::Specialized;
using namespace CSGeneral;

// --------------------------------------------------------------------------
// Raw message data support.
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CICreateMessageData", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
char* CreateMessageData(char* Message);

[DllImport("ComponentInterface2.dll", EntryPoint = "CIDeleteMessageData", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void DeleteMessageData(char* MessageData);


// --------------------------------------------------------------------------
// Boolean support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIPackBool", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void pack(char* messageData, Boolean Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackBool", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
bool unpack(char* messageData, Boolean% Data, String^ VariableName);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackBoolWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Boolean% Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIMemorySizeBool", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
unsigned memorySize(bool Data);
inline String^ DDML(Boolean Data)
	{
	return "<type kind=\"boolean\"/>";
	}


// --------------------------------------------------------------------------
// Int32 support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIPackInt", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void pack(char* messageData, Int32 Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackInt", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
bool unpack(char* messageData, Int32% Data, String^ VariableName);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackIntWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Int32% Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIMemorySizeInt", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
unsigned memorySize(int Data);
inline String^ DDML(Int32 Data)
	{
	return "<type kind=\"integer4\"/>";
	}



// --------------------------------------------------------------------------
// Single support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIPackSingle", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void pack(char* messageData, Single Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackSingle", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
bool unpack(char* messageData, Single% Data, String^ VariableName);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackSingleWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Single% Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIMemorySizeSingle", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
unsigned memorySize(float Data);
inline String^ DDML(Single Data)
	{
	return "<type kind=\"single\"/>";
	}


// --------------------------------------------------------------------------
// Double support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIPackDouble", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void pack(char* messageData, Double Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackDouble", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
bool unpack(char* messageData, Double% Data, String^ VariableName);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackDoubleWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Double% Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIMemorySizeDouble", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
unsigned memorySize(double Data);
inline String^ DDML(Double Data)
	{
	return "<type kind=\"double\"/>";
	}


// --------------------------------------------------------------------------
// String support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIPackString", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void packRaw(char* messageData, String^ Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackString", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
bool unpack(char* messageData, System::Text::StringBuilder^ Data, String^ VariableName);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackStringWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, System::Text::StringBuilder^ Data);
[DllImport("ComponentInterface2.dll", EntryPoint = "CIMemorySizeString", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
unsigned memorySize(String^ Data);
inline String^ DDML(String^ Data)
	{
	return "<type kind=\"string\"/>";
	}
inline void pack(char* messageData, String^ Data)
	{
	::packRaw(messageData, Data);
	}
inline void unpack(char* messageData, String^% st, String^ VariableName)
	{
	System::Text::StringBuilder^ Contents = gcnew System::Text::StringBuilder(5000);
	::unpack(messageData, Contents, VariableName);
	st = Contents->ToString();
	}
inline void unpackWithConverter(char* messageData, String^% st)
	{
	System::Text::StringBuilder^ Contents = gcnew System::Text::StringBuilder(5000);
	::unpackWithConverter(messageData, Contents);
	st = Contents->ToString();
	}

// --------------------------------------------------------------------------
// DateTime support
// --------------------------------------------------------------------------
inline void pack(char* messageData, DateTime Date)
   {
   ::pack(messageData, DateUtility::dateTimeToJulianDate(Date));
   }

inline bool unpack(char* messageData, DateTime% Date, String^ VariableName)
   {
   Int32 JulianDate;
   ::unpack(messageData, JulianDate, VariableName);
   Date = DateUtility::JulianDateToDateTime(JulianDate);
   }

inline void unpackWithConverter(char* messageData, DateTime% Date)
   {
   Int32 JulianDate;
   ::unpackWithConverter(messageData, JulianDate);
   Date = DateUtility::JulianDateToDateTime(JulianDate);
   }
inline unsigned memorySize(DateTime Date)
   {
   Int32 JulianDate = 0;
   return ::memorySize(JulianDate);
   }
inline String^ DDML(DateTime Date)
	{
	return "<type kind=\"integer4\"/>";
	}

// --------------------------------------------------------------------------
// Bool array support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackBoolArrayWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Boolean Data[], int& numValues);

// --------------------------------------------------------------------------
// Int32 array support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackIntArrayWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Int32 Data[], int& numValues);

// --------------------------------------------------------------------------
// Single array support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackSingleArrayWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Single Data[], int& numValues);

// --------------------------------------------------------------------------
// Double array support
// --------------------------------------------------------------------------

[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackDoubleArrayWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackWithConverter(char* messageData, Double Data[], int& numValues);

// --------------------------------------------------------------------------
// String array support
// --------------------------------------------------------------------------
[DllImport("ComponentInterface2.dll", EntryPoint = "CIUnPackStringArrayWithConverter", CharSet=CharSet::Ansi, CallingConvention=CallingConvention::StdCall)]
void unpackStringArrayWithConverter(char* messageData, System::Text::StringBuilder^ Data);


// --------------------------------------------------------------------------
// General array support.
// --------------------------------------------------------------------------
template <class T>
void pack(char* messageData, array<T>^ values)
   {
   ::pack(messageData, values->Length);
   for (int i = 0; i < values->Length; i++)
	  ::pack(messageData, values[i]);
   }
template <class T>
void unpack(char* messageData, array<T>^% values, String^ VariableName)
   {
   int count = 0;
   ::unpack(messageData, count, "");
   values = gcnew array<T>(count);
   for (int i = 0; i < count; i++)
	  ::unpack(messageData, values[i], VariableName);
   }
template <class T>
inline void unpackWithConverter(char* messageData, array<T>^% values)
   {
   T data[1000];
   int count = 0;
   ::unpackWithConverter(messageData, data, count);
   values->Resize(values, count);
   for (int i = 0; i < count; i++)
      values[i] = data[i];
   }
template <> // template specialisation for String^
inline void unpackWithConverter(char* messageData, array<String^>^% values)
   {
	System::Text::StringBuilder^ data = gcnew System::Text::StringBuilder(5000);
   ::unpackStringArrayWithConverter(messageData, data);
   StringCollection^ allValues = CSGeneral::StringManip::SplitStringHonouringQuotes(data->ToString(), ",");
   values->Resize(values, allValues->Count);
   for (int i = 0; i < allValues->Count; i++)
      values[i] = allValues[i]->Replace("\"","");
   }

// We made need to recurse into structures to build them completely.
// THIS NEEDS MORE WORK!
// Creating a type which contains arrays of structures which contain arrays of structures
// gets pretty messy. This code does NOT handle all possible cases correctly,
// but does handle those currently is use. EJZ
static Object^ createStructure(Type^ targetType)
{
	Object^ target = Activator::CreateInstance(targetType);
	if (!targetType->IsValueType)
	{
		array<FieldInfo^>^ childFields = targetType->GetFields();
		for (int i = 0; i < childFields->Length; i++)
		{
			Type^ childType = childFields[i]->FieldType;
			if (!childType->IsArray && !childType->IsValueType && !childType->Equals(System::String::typeid))
			{
				Object^ newChild = createStructure(childType);
				childFields[i]->SetValue(target, newChild);
			}
		}
	}
	return target;
}

template <class T>
void unpackArrayOfStructures(char* messageData, array<T^>^% values)
   {
   int count = 0;
   ::unpack(messageData, count, "");
   values = gcnew array<T^>(count);
   for (int i = 0; i < count; i++)
	  {
	  values[i] = (T^)createStructure(T::typeid);
	  ::unpack(messageData, values[i]);
	  }
   }
template <class T>
unsigned memorySize(array<T>^ values)
   {
   unsigned size = 4;
   for (int i = 0; i < values->Length; i++)
      size += ::memorySize(values[i]);
   return size;
   }
template <class T>
#pragma warning(disable:4700)
String^ DDML(array<T>^ values)
	{
	T Dummy;
	String^ TypeString = ::DDML(Dummy);
	return TypeString->Substring(0, TypeString->Length-2) + " array=\"T\"/>";
	}


template <class T>
public ref class WrapBuiltInVariable : ApsimType
   {
   // --------------------------------------------------------------------
   // This class wraps a FactoryProperty and a built in type (e.g. Single, 
   // Double etc). It then makes it look like an ApsimType with pack,
   // unpack methods etc.
   // --------------------------------------------------------------------
   public:
      T Value;
      WrapBuiltInVariable()
         {
         }
		virtual void pack(char* messageData)
			{
			::pack(messageData, (T) Value);
			}
		virtual void unpack(char* messageData)
			{
			::unpackWithConverter(messageData, Value);
			}
		virtual unsigned memorySize()
			{
			return ::memorySize((T) Value);
			}
      virtual String^ DDML()
         {
         return ::DDML(Value);
         }
	};
	
		