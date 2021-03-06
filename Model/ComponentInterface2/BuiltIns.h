//---------------------------------------------------------------------------
#ifndef BuiltInsH
#define BuiltInsH

#include <stdio.h>
#include <stdlib.h>
#include <stdint.h>
#include <stdexcept>

#include <ComponentInterface2/MessageData.h>
#include <ComponentInterface2/FortranString.h>

// ------ boolean ------
inline unsigned int memorySize(const bool& value)
   {return 1;}
inline void unpack(MessageData& messageData, bool& value)
   {
   value = *((char*)messageData.ptr()) != 0;
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, bool value)
   {
   *((char*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(bool value)
   {return "<type kind=\"boolean\"/>";}

inline void Convert(bool source, bool& dest)   {dest = source;}
inline void Convert(int source, bool& dest)    {dest = source != 0;}
inline void Convert(float source, bool& dest)  {dest = source != 0.0;}
inline void Convert(double source, bool& dest) {dest = source != 0.0;}
inline void Convert(const std::string& source, bool& dest)
   {
   char *chk;
   dest = strtol(source.c_str(), &chk, 10) != 0;
   if (chk == source.c_str()) {throw std::runtime_error("Cannot parse bool from string \"" + source + "\"");}
   }

// ------ int ------
inline unsigned int memorySize(const int& value)
   {return 4;}
inline void unpack(MessageData& messageData, int& value)
   {
   value = *((int*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, int value)
   {
   *((int*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(int value)
   {return "<type kind=\"integer4\"/>";}
inline void Convert(bool source, int& dest)   {dest = source;}
inline void Convert(int source, int& dest)    {dest = source;}
inline void Convert(float source, int& dest)  {dest = (int)source;}
inline void Convert(double source, int& dest) {dest = (int)source;}
inline void Convert(const std::string& source, int& dest)
   {
   char *chk;
   dest = (int) strtol(source.c_str(), &chk, 10);
   if (chk == source.c_str()) {throw std::runtime_error("Cannot parse int from string \"" + source + "\"");}
   }

// ------ unsigned ------
inline unsigned int memorySize(const unsigned& value)
   {return sizeof(unsigned);}
inline void unpack(MessageData& messageData, unsigned& value)
   {
   value = *((unsigned*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, const unsigned value)
   {
   *((unsigned*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(const unsigned value)
   {return (sizeof(unsigned) == 4) ? "<type kind=\"integer4\"/>" : "<type kind=\"integer8\"/>";}

inline void Convert(bool source, uintptr_t& dest)   {dest = source;}
inline void Convert(int source, uintptr_t& dest)    {dest = source;}
inline void Convert(float source, uintptr_t& dest)  {dest = (uintptr_t)source;}
inline void Convert(double source, uintptr_t& dest) {dest = (uintptr_t)source;}
inline void Convert(const std::string& source, uintptr_t& dest)
   {
   char *chk;
   dest = (uintptr_t) strtoul(source.c_str(), &chk, 10);
   if (chk == source.c_str()) {throw std::runtime_error("Cannot parse long int from string \"" + source + "\"");}
   }
// ------ float ------
inline unsigned int memorySize(const float& value)
   {return 4;}
inline void unpack(MessageData& messageData, float& value)
   {
   value = *((float*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, float value)
   {
   *((float*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(float value)
   {return "<type kind=\"single\"/>";}
inline void Convert(bool source, float& dest)  {dest = source;}
inline void Convert(int source, float& dest)   {dest = (float) source;}
inline void Convert(float source, float& dest) {dest = source;}
inline void Convert(double source, float& dest){dest = (float) source;}
inline void Convert(const std::string& source, float& dest)
   {
   char *chk;
   dest = (float) strtod(source.c_str(), &chk);
   if (chk == source.c_str()) {throw std::runtime_error("Cannot parse float from string \"" + source + "\"");}
   }

// ------ double ------
inline unsigned int memorySize(const double& value)
   {return 8;}
inline void unpack(MessageData& messageData, double& value)
   {
   value = *((double*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, double value)
   {
   *((double*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(double value)
   {return "<type kind=\"double\"/>";}
inline void Convert(bool source, double& dest)  {dest = source;}
inline void Convert(int source, double& dest)   {dest = source;}
inline void Convert(float source, double& dest) {dest = source;}
inline void Convert(double source, double& dest) {dest = source;}
inline void Convert(const std::string& source, double& dest)
   {
   char *chk;
   dest = (double) strtod(source.c_str(), &chk);
   if (chk == source.c_str()) {throw std::runtime_error("Cannot parse double from string \"" + source + "\"");}
   }

// ------ char ------
inline unsigned int memorySize(const char& value)
   {return 1;}
inline void unpack(MessageData& messageData, char& value)
   {
   value = *((char*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, char value)
   {
   *((char*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(char value)
   {return "<type kind=\"char\"/>";}

// ------ wchar ------
typedef wchar_t WCHAR;
inline unsigned int memorySize(const WCHAR& value)
   {return 2;}
inline void unpack(MessageData& messageData, WCHAR& value)
   {
   value = *((WCHAR*)messageData.ptr());
   messageData.movePtrBy(memorySize(value));
   }
inline void pack(MessageData& messageData, WCHAR value)
   {
   *((WCHAR*)messageData.ptr()) = value;
   messageData.movePtrBy(memorySize(value));
   }
inline std::string DDML(WCHAR value)
   {return "<type kind=\"wchar\"/>";}

// ------ std::string ------
inline unsigned int memorySize(const std::string& value)
   {return (unsigned)value.length() + 4;}
inline void unpack(MessageData& messageData, std::string& value)
   {
   int numChars = 0;
   unpack(messageData, numChars);
   value = std::string(messageData.ptr(), numChars);
   messageData.movePtrBy(numChars);
   }
inline void pack(MessageData& messageData, const std::string& value)
   {
   pack(messageData, (int) value.length());
   messageData.copyFrom(value.c_str(), (unsigned)value.length());
   }
inline std::string DDML(const std::string& value)
   {return "<type kind=\"string\"/>";}
inline void Convert(bool source, std::string& dest)  {if (source) dest = "1"; else dest = "0"; }
inline void Convert(int source, std::string& dest)   {dest = std::to_string((long long)source);}
inline void Convert(const std::string& source, std::string& dest) {dest = source;}

#if __cplusplus <= 199711L
#include <stdarg.h>
// Scrap this when we move from VS2010
#define snprintf c99_snprintf


inline int c99_vsnprintf(char* str, size_t size, const char* format, va_list ap)
{
    int count = -1;

    if (size != 0)
        count = _vsnprintf_s(str, size, _TRUNCATE, format, ap);
    if (count == -1)
        count = _vscprintf(format, ap);

    return count;
}

inline int c99_snprintf(char* str, size_t size, const char* format, ...)
{
    int count;
    va_list ap;

    va_start(ap, format);
    count = c99_vsnprintf(str, size, format, ap);
    va_end(ap);

    return count;
}

#endif // _MSC_VER

inline void Convert(float source, std::string& dest)
   {
#if __cplusplus <= 199711L
   // Scrap this when we move from VS2010
   char st[64];
   snprintf(st, 63, "%f", source);
   st[63] = '\0';
   dest = st;
#else
   dest = std::to_string(source);
#endif
   }
inline void Convert(double source, std::string& dest)
   {
#if __cplusplus <= 199711L
   // Scrap this when we move from VS2010
   char st[64];
   snprintf(st, 63, "%lf", source);
   st[63] = '\0';
   dest = st;
#else
   dest = std::to_string(source);
#endif
   }

// ------ std::vector ------
template <class T>
inline unsigned int memorySize(const std::vector<T>& values)
   {
   unsigned size = 4;
   for (unsigned i = 0; i != values.size(); i++)
      size += memorySize(values[i]);
   return size;
   }
template <class T>
inline void unpack(MessageData& messageData, std::vector<T>& values)
   {
   values.erase(values.begin(), values.end());
   int numValues;
   unpack(messageData, numValues);
   for (int i = 0; i < numValues; i++)
      {
      T value;
      unpack(messageData, value);
      values.push_back(value);
      }
   }
template <class T>
inline void pack(MessageData& messageData, const std::vector<T>& values)
   {
   int numValues = (int)values.size();

   pack(messageData, numValues);
   for (int i = 0; i < (int) values.size(); i++)
      pack(messageData, values[i]);
   }
template <class T>
inline std::string DDML(std::vector<T>& value)
   {
   T dummy = T();
   std::string ddml = DDML(dummy);
   addAttributeToXML(ddml, "array=\"T\"");
   return ddml;
   }
template <class FROM>
inline void Convert(const std::vector<FROM>& from, std::string& to)
   {
   to = "";
   for (unsigned i = 0; i < from.size(); i++)
      {
	  std::string st;
      ::Convert(from[i], st);
	  if (to != "")
	     to += " ";
	  to += st;
	  }
   }
 template <class FROM, class TO>
inline void Convert(const std::vector<FROM>& from, TO& to)
   {
   if (from.size() != 1)
      throw std::runtime_error("Too many values to convert from vector to scalar");
   ::Convert(from[0], to);
   }
   
template <>
inline void Convert(const std::vector<std::string>& from, std::string& to)
   {
   to = "";
   for (unsigned i = 0; i != from.size(); i++)
      {
      if (to != "")
         to += " ";
      to += from[i];
      }
   }
template <class FROM, class TO>
inline void Convert(const std::vector<FROM>& from, std::vector<TO>& toVector)
   {
   TO to;
   toVector.clear();
   for (unsigned i = 0; i != from.size(); i++)
      {
      ::Convert(from[i], to);
      toVector.push_back(to);
      }
   }
template <class FROM, class TO>
inline void Convert(const FROM& from, std::vector<TO>& toVector)
   {
   TO to;
   ::Convert(from, to);
   toVector.push_back(to);
   }
#endif
