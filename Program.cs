static class Program {
	static readonly Random random = new Random();
	const string DEFAULT_PATH = @"C:\Program Files (x86)\Steam\steamapps\common\Morrowind\Data Files\Morrowind.bsa";
	const string output_directory = "output";
	static int Main(string[] args){
		string path = 0 < args.Length ? args[0] : DEFAULT_PATH;
		byte[] rawData = File.ReadAllBytes(path); // .Skip(24)
		uint magic = BitConverter.ToUInt32(rawData.Take(4).ToArray());
		if (magic != 0x00000100){
			Console.WriteLine("magic number is bad magic :<");
			return 1;
		}
		// Offset of the hash table in the file, minus the header size (12).
		uint hash_table_offset = BitConverter.ToUInt32(rawData.Skip(4).Take(4).ToArray());
		uint file_count = BitConverter.ToUInt32(rawData.Skip(8).Take(4).ToArray());
		int offset = 12;
		// file sizes and offsets
		uint[,] file_sizes_and_offsets = new uint[file_count, 2];
		for (int i = 0; i < file_count; i++){
			// size
			file_sizes_and_offsets[i, 0] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
			// offset
			file_sizes_and_offsets[i, 1] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
		}
		// name offsets
		uint[] name_offsets = new uint[file_count];
		for (int i = 0; i < file_count; i++){
			name_offsets[i] = BitConverter.ToUInt32(rawData.Skip(offset).Take(4).ToArray());
			offset += 4;
		}
		// strings
		int name_section_offset = offset;
		string[] names = new string[file_count];
		for (int i = 0; i < file_count; i++){
			int end = (int)(i == file_count - 1 ? hash_table_offset : name_offsets[i+1]);
			names[i] = System.Text.Encoding.ASCII.GetString(rawData.Skip(name_section_offset + (int)name_offsets[i]).Take(end - (int)name_offsets[i]).ToArray());
		}
		// hashes
		ulong[] hashes = new ulong[file_count];
		int data_offset = 0;
		for (int i = 0; i < file_count; i++)
			hashes[i] = BitConverter.ToUInt64(rawData.Skip(data_offset = 12 + (int)hash_table_offset + 8*i).Take(8).ToArray());
		// data
		data_offset += 8;
		byte[][] data = new byte[file_count][];
		for (int i = 0; i < file_count; i++)
			data[i] = rawData.Skip(data_offset + (int)file_sizes_and_offsets[i, 1]).Take((int)file_sizes_and_offsets[i, 0]).ToArray();
		// save
		for (int i = 0; i < names.Length; i++){
			string name = 255 < names[i].Length ? random.Next().ToString() : names[i];
			Console.WriteLine($"Writing {i+1}/{names.Length} to {output_directory}: {name}");
			WriteFile(output_directory + "\\" + name, data[i]);
		}
		// its oki
		return 0;
	}
	static void WriteFile(string name, byte[] data){
		name = name.Replace("\0", "");
		string? directory = Path.GetDirectoryName(name);
		if (directory is not null && !Directory.Exists(directory))
			Directory.CreateDirectory(directory);
		File.WriteAllBytes(name, data);
	}
}