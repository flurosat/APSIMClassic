﻿using System;
using System.Reflection;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Linq;
using System.Text;
using ModelFramework;
using CSGeneral;

/// <remarks>
/// This partial class contains the various methods to handle patches
/// </remarks>
public partial class SoilNitrogen
{
	/// <summary>
	/// Handles the addition of new CNPatches
	/// </summary>
	/// <param name="PatchtoAdd">Patch data</param>
	private void AddNewCNPatch(AddSoilCNPatchwithFOMType PatchtoAdd)
	{
		// Data passed from OnAddSoilCNPatch event:
		//.Sender: the name of the module that raised this event
		//.DepositionType: the type of deposition:
		//  - ToAllPaddock: No patch is created, add stuff as given to all patches. It is the default;
		//  - ToSpecificPatch: No patch is created, add stuff to given patches;
		//		(recipient patch is given using its index or name; if not supplied, defaults to homogeneous)
		//  - ToNewPatch: create new patch based on an existing patch, add stuff to created patch;
		//		- recipient or base patch is given using index or name; if not supplied, new patch will be based on the base/Patch[0];
		//      - patches are only created is area is larger than a minimum (minPatchArea);
		//      - new areas are proportional to existing patches;
		//  - NewOverlappingPatches: create new patch(es), these overlap with all existing patches, add stuff to created patches;
		//		(new patches are created only if their area is larger than a minimum (minPatchArea))
		//.AffectedPatches_id (AffectedPatchesByIndex): the index of the existing patches affected by new patch
		//.AffectedPatches_nm (AffectedPatchesByName): the name of the existing patches affected by new patch
		//.AreaNewPatch: the relative area (fraction) of new patches (0-1)
		//.PatchName: the name(s) of the patch(es) being created
		//.Water: amount of water to add per layer (mm), not handled here
		//.Urea: amount of urea to add per layer (kgN/ha)
		//.NH4: amount of ammonium to add per layer (kgN/ha)
		//.NO3: amount of nitrate to add per layer (kgN/ha)
		//.POX: amount of POx to add per layer (kgP/ha), not handled here
		//.SO4: amount of SO4 to add per layer (kgS/ha), not handled here
		//.AshAlk: ash amount to add per layer (mol/ha), not handled here
		//.FOM: fresh organic matter to add, per fom pool
		//   .name: name of given pool being altered
		//   .type: type of the given pool being altered (not used here)
		//   .Pool[]: info about FOM pools being added
		//      .type: type of the given pool being altered (not used here)
		//      .type: type of the given pool being altered (not used here)
		//      .C: amount of carbon in given pool to add per layer (kgC/ha)
		//      .N: amount of nitrogen in given pool to add per layer (kgN/ha)
		//      .P: amount of phosphorus (kgC/ha), not handled here
		//      .S: amount of sulphur (kgC/ha), not handled here
		//      .AshAlk: amount of alkaline ash (kg/ha), not handled here


		List<int> idPatchesJustAdded = new List<int>();	// list of IDs of patches created (exclude patches that would be too small)
		List<int> idPatchesToDelete = new List<int>();	//list of IDs of existing patches that became too small and need to be deleted
		List<int> idPatchesAffected;					//list of IDs of patches affected by new addition

		// 1. get the list of id's of patches which are affected by this addition, and the area affected
		double AreaAffected = 0;
		if (PatchtoAdd.DepositionType.ToLower() == "ToNewPatch".ToLower())
		{  // check which patches are affected
			idPatchesAffected = CheckPatchIDs(PatchtoAdd.AffectedPatches_id, PatchtoAdd.AffectedPatches_nm);
			for (int i = 0; i < idPatchesAffected.Count; i++)
				AreaAffected += Patch[idPatchesAffected[i]].RelativeArea;
		}
		else if (PatchtoAdd.DepositionType.ToLower() == "NewOverlappingPatches".ToLower())
		{  // all patches are affected
			idPatchesAffected = new List<int>();
			for (int k = 0; k < Patch.Count; k++)
				idPatchesAffected.Add(k);
			AreaAffected = 1.0;
		}
		else
		{
			idPatchesAffected = new List<int>();
		}

		// check that total area of affected patches is larger than new patch area
		if (AreaAffected < PatchtoAdd.AreaNewPatch)
		{
			// Existing area is smaller than new patch area, cannot continue
			writeMessage(" AddSoilCNPatch - area of selected patches (" + AreaAffected.ToString("#0.00#")
							   + ") is smaller than area of new patch(" + PatchtoAdd.AreaNewPatch.ToString("#0.00#") + "). Command will be ignored");
		}
		else
		{  // check the area for each patch
			for (int i = 0; i < idPatchesAffected.Count; i++)
			{
				double OldPatch_OldArea = Patch[idPatchesAffected[i]].RelativeArea;
				double NewPatch_NewArea = PatchtoAdd.AreaNewPatch * (OldPatch_OldArea / AreaAffected);
				double OldPatch_NewArea = OldPatch_OldArea - NewPatch_NewArea;
				if (NewPatch_NewArea < MinimumPatchArea)
				{  // area to create is too small, patch will not be created
					Console.WriteLine(Clock.Today.ToString("dd MMMM yyyy") + "(Day of year="
						+ Clock.Today.DayOfYear.ToString() + "), SoilNitrogen.AddCNPatch:");
					Console.WriteLine("   attempt to create a new patch with area too small or negative ("
						+ NewPatch_NewArea.ToString("#0.00#") + "). The patch will not be created.");
				}
				else if (OldPatch_NewArea < MinimumPatchArea)
				{  // remaining area is too small or negative, patch will be created but old one will be deleted
					Console.WriteLine(Clock.Today.ToString("dd MMMM yyyy") + "(Day of year="
						+ Clock.Today.DayOfYear.ToString() + "), SoilNitrogen.AddCNPatch:");
					Console.WriteLine(" attempt to set the area of existing patch(" + idPatchesAffected[i].ToString()
						+ ") to a value too small or negative (" + OldPatch_NewArea.ToString("#0.00#")
						+ "). The patch will be eliminated.");

					// mark old patch for deletion
					idPatchesToDelete.Add(idPatchesAffected[i]);

					// create new patch based on old one - the original one will be deleted later
					ClonePatch(idPatchesAffected[i]);
					int k = Patch.Count - 1;
					if (PatchtoAdd.AreaNewPatch > 0)
					{  // a name was supplied
						Patch[k].PatchName = PatchtoAdd.AreaNewPatch + "_" + i.ToString();
					}
					else
					{  // use default naming
						Patch[k].PatchName = "patch" + k.ToString();
					}
					idPatchesJustAdded.Add(k);
				}
				else
				{
					// create new patch by spliting an existing one
					ClonePatch(idPatchesAffected[i]);
					Patch[idPatchesAffected[i]].RelativeArea = OldPatch_NewArea;
					int k = Patch.Count - 1;
					Patch[k].RelativeArea = NewPatch_NewArea;
					if (PatchtoAdd.PatchName.Length > 0)
					{  // a name was supplied
						Patch[k].PatchName = PatchtoAdd.AreaNewPatch + "_" + i.ToString();
					}
					else
					{  // use default naming
						Patch[k].PatchName = "patch" + k.ToString();
					}
					idPatchesJustAdded.Add(k);
					writeMessage("create new patch, with area = " + NewPatch_NewArea.ToString("#0.00#") + ", based on existing patch("
						+ idPatchesAffected[i].ToString() + ") - Old area = " + OldPatch_OldArea.ToString("#0.00#") + ", new area = "
						+ OldPatch_NewArea.ToString("#0.00#"));
				}
			}
		}

		// add the stuff to patches just created
		AddStuffToPatches(idPatchesJustAdded, PatchtoAdd);

		// delete the patches in excess
		if (idPatchesToDelete.Count > 0)
			DeletePatches(idPatchesToDelete);

	}

	/// <summary>
	/// Clone an existing patch. That is, creates a new patch (k) based on an existing one (j)
	/// </summary>
	/// <param name="k">id of patch to be cloned</param>
	private void ClonePatch(int j)
	{
		// create new patch
		soilCNPatch newPatch = new soilCNPatch(this);
		Patch.Add(newPatch);
		int k = Patch.Count - 1;

		// set the size of arrays
		Patch[k].ResizeLayeredVariables(dlayer.Length);

		// copy the state variables from original patch in to the new one
		CopyCNValuesToPatch(k, j);
	}

	/// <summary>
	/// Check patch state, get the diffs between them, and merge them if possible
	/// </summary>
	/// <remarks>
	/// We're testing three different ways to compare/merge patches:
	///  - CompareAll: All patches are compared to each other before they are merged
	///  - CompareBase: All patches are compare to base first, then merged, then compared again
	///  - CompareMerge: Patches are compare and merged at once if deemed equal, then compare to next
	/// </remarks>
	private void CheckPatchAutoAmalgamation()
	{
		int nPatches = Patch.Count;

		// 1. get the list of names of existing patches, this will be used as reference and adjusted as patches are merged
		List<string> ExistingPatches = new List<string>();
		for (int k = 0; k < nPatches; k++)
			ExistingPatches.Add(Patch[k].PatchName);

		if (PatchAmalgamationApproach.ToLower() == "CompareAll".ToLower())
		{
			// A2. initialise the lists, for each existing patch, of patches to be merged to it
			List<List<int>> MergingPatches = new List<List<int>>();
			for (int k = 0; k < nPatches; k++)
				MergingPatches.Add(new List<int>());

			// A3. go through all patches and check whether they are similar enough to any other
			for (int k = 0; k < nPatches - 1; k++)	 //  this will go to all but the last patch, as it has no other patch to be compared with
			{
				if (ExistingPatches.Contains(Patch[k].PatchName))  // skip already selected patches
				{
					for (int j = k + 1; j < nPatches; j++)  // compare to all other subsequent patches
						if (PatchesAreEqual(k, j))
						{
							MergingPatches[k].Add(j);		// add patch j to the list being merged into patch k
							ExistingPatches.RemoveAt(j);	// remove name of patch j from the reference list
						}
				}
				// else {} go to next patch
			}

			int nPatchesDeleted = Patch.Count - ExistingPatches.Count;

			// A4. do the actual merging (copy values from and deleted merging patches)
			if (nPatchesDeleted > 0)
			{
				List<int> PatchesToDelete = new List<int>();
				// A4.1. Copy values between patches
				for (int k = 0; k < Patch.Count - 1; k++)
				{
					if (MergingPatches[k].Count > 0)
						for (int i = 0; i < MergingPatches[k].Count; i++)
						{
							int j = MergingPatches[k][i];
							MergeCNValues(k, j);
							PatchesToDelete.Add(j);
							writeMessage("merging patch(" + j + ") into patch(" + k + "). New patch area = " + Patch[k].RelativeArea.ToString("#0.00#"));
						}
				}
				// A4.2. Delete merged patches
				DeletePatches(PatchesToDelete);
				PatchesToDelete.Clear();
			}
		}
		else if (PatchAmalgamationApproach.ToLower() == "CompareBase".ToLower())
		{
			// B2. initialise the list of patches to be merged/deleted
			List<int> PatchesToDelete = new List<int>();

			// B3. go through all patches and check whether they are similar enough to any other
			int k = 0;
			do
			{
				for (int j = k + 1; j < nPatches; j++)  // compare to all other subsequent patches
					if (PatchesAreEqual(k, j))
						PatchesToDelete.Add(j);		// add patch j to the list being merged into patch k

				// B4. do the actual merging (copy values from and deleted merging patches)
				if (PatchesToDelete.Count > 0)
				{
					// B4.1. Copy values between patches
					for (int i = 0; i < PatchesToDelete.Count; i++)
					{
						int j = PatchesToDelete[i];
						MergeCNValues(k, j);
						writeMessage("merging patch(" + j + ") into patch(" + k + "). New patch area = " + Patch[k].RelativeArea.ToString("#0.00#"));
						ExistingPatches.RemoveAt(j);	// remove name of patch j from the reference list
					}
					// B4.2. Delete merged patches
					DeletePatches(PatchesToDelete);
					PatchesToDelete.Clear();
					nPatches = Patch.Count;
				}
				else
				{
					k += 1;	// go to next patch
				}
			} while (k < nPatches - 1);
		}
		else if (PatchAmalgamationApproach.ToLower() == "CompareMerge".ToLower())
		{
			// C2. initialise the list of patches to be deleted
			List<int> PatchesToDelete = new List<int>();

			// C3. go through all patches and check whether they are similar enough to any other, merge if they are
			int k = 0;
			do
			{
				for (int j = k + 1; j < nPatches; j++)
				{
					if (PatchesAreEqual(k, j))
					{
						// C4.1. Copy values between patches
						MergeCNValues(k, j);
						writeMessage("merging patch(" + j + ") into patch(" + k + "). New patch area = " + Patch[k].RelativeArea.ToString("#0.00#"));
						PatchesToDelete.Add(j);
					}
				}
				// C4.2. Delete merged patches
				if (PatchesToDelete.Count > 0)
				{
					DeletePatches(PatchesToDelete);
					PatchesToDelete.Clear();
					nPatches = Patch.Count;
				}
				else
				{
					k += 1;	// go to next patch
				}
			} while (k < nPatches - 1);
		}
	}

	/// <summary>	
	/// Controls the merging of a list of patches into a single one
	/// </summary>
	/// <param name="PatchesToMerge">List of patches to merge</param>
	private void AmalgamatePatches(List<int> PatchesToMerge)
	{
		List<int> PatchesToDelete = new List<int>();
		int k = PatchesToMerge[0];  // receiver patch
		while (PatchesToMerge.Count > 1)
		{
			//MergePatches(PatchesToMerge[0], PatchesToMerge[1]); 
			int j = PatchesToMerge[1];  // Patch being merged
			MergeCNValues(k, j);
			writeMessage("merging patch(" + j + ") into patch(" + k + "). New patch area = " + Patch[k].RelativeArea.ToString("#0.00#"));
			PatchesToDelete.Add(j);								// add reference to patch to be deleted
			PatchesToMerge.RemoveAt(1);                         // delete reference to patch[j]
		}
		// delete merged patches
		DeletePatches(PatchesToDelete);
	}

	/// <summary>
	/// Delete patches in the list
	/// </summary>
	/// <param name="PatchesToDelete">List of patches to delete</param>
	private void DeletePatches(List<int> PatchesToDelete)
	{
		// go backwards so that the id of patches to delete do not change after each deletion
		for (int i = PatchesToDelete.Count - 1; i >= 0; i--)
		{
			Patch.RemoveAt(PatchesToDelete[i]);
		}
	}

	/// <summary>
	/// Copy the state variables from one patch (j) to another one (k), using a multiplying factor
	/// </summary>
	/// <param name="k">The id of patch where values are copied to</param>
	/// <param name="j">The id of patch where values are copied from</param>
	/// <param name="MultiplyingFactor">A multiplying factor (optional)</param>
	private void CopyCNValuesToPatch(int k, int j, double MultiplyingFactor = 1.0)
	{
		for (int layer = 0; layer < dlayer.Length; layer++)
		{
			// Mineral N
			Patch[k].urea[layer] += Patch[j].urea[layer] * MultiplyingFactor;
			Patch[k].nh4[layer] += Patch[j].nh4[layer] * MultiplyingFactor;
			Patch[k].no3[layer] += Patch[j].no3[layer] * MultiplyingFactor;
			Patch[k].TodaysInitialNH4[layer] += Patch[j].TodaysInitialNH4[layer] * MultiplyingFactor;
			Patch[k].TodaysInitialNO3[layer] += Patch[j].TodaysInitialNO3[layer] * MultiplyingFactor;

			// Organic C and N
			for (int pool = 0; pool < 3; pool++)
			{
				Patch[k].fom_c[pool][layer] += Patch[j].fom_c[pool][layer] * MultiplyingFactor;
				Patch[k].fom_n[pool][layer] += Patch[j].fom_n[pool][layer] * MultiplyingFactor;
			}
			Patch[k].biom_c[layer] += Patch[j].biom_c[layer] * MultiplyingFactor;
			Patch[k].biom_n[layer] += Patch[j].biom_n[layer] * MultiplyingFactor;
			Patch[k].hum_c[layer] += Patch[j].hum_c[layer] * MultiplyingFactor;
			Patch[k].hum_n[layer] += Patch[j].hum_n[layer] * MultiplyingFactor;
			Patch[k].inert_c[layer] += Patch[j].inert_c[layer] * MultiplyingFactor;
			Patch[k].inert_n[layer] += Patch[j].inert_n[layer] * MultiplyingFactor;
		}
	}

	/// <summary>
	/// Merge the state variables from one patch (j) to another one (k), include area
	/// </summary>
	/// <param name="k">The id of patch where values are copied to</param>
	/// <param name="j">The id of patch where values are copied from</param>
	private void MergeCNValues(int k, int j)
	{

		double NewPatchArea = Patch[k].RelativeArea + Patch[j].RelativeArea;
		for (int layer = 0; layer < dlayer.Length; layer++)
		{
			// Mineral N
			Patch[k].urea[layer] = (Patch[k].urea[layer] * Patch[k].RelativeArea + Patch[j].urea[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].nh4[layer] = (Patch[k].nh4[layer] * Patch[k].RelativeArea + Patch[j].nh4[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].no3[layer] = (Patch[k].no3[layer] * Patch[k].RelativeArea + Patch[j].no3[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].TodaysInitialNH4[layer] = (Patch[k].TodaysInitialNH4[layer] * Patch[k].RelativeArea + Patch[j].TodaysInitialNH4[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].TodaysInitialNO3[layer] = (Patch[k].TodaysInitialNO3[layer] * Patch[k].RelativeArea + Patch[j].TodaysInitialNO3[layer] * Patch[j].RelativeArea) / NewPatchArea;

			// Organic C and N
			for (int pool = 0; pool < 3; pool++)
			{
				Patch[k].fom_c[pool][layer] = (Patch[k].fom_c[pool][layer] * Patch[k].RelativeArea + Patch[j].fom_c[pool][layer] * Patch[j].RelativeArea) / NewPatchArea;
				Patch[k].fom_n[pool][layer] = (Patch[k].fom_n[pool][layer] * Patch[k].RelativeArea + Patch[j].fom_n[pool][layer] * Patch[j].RelativeArea) / NewPatchArea;
			}
			Patch[k].biom_c[layer] = (Patch[k].biom_c[layer] * Patch[k].RelativeArea + Patch[j].biom_c[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].biom_n[layer] = (Patch[k].biom_n[layer] * Patch[k].RelativeArea + Patch[j].biom_n[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].hum_c[layer] = (Patch[k].hum_c[layer] * Patch[k].RelativeArea + Patch[j].hum_c[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].hum_n[layer] = (Patch[k].hum_n[layer] * Patch[k].RelativeArea + Patch[j].hum_n[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].inert_c[layer] = (Patch[k].inert_c[layer] * Patch[k].RelativeArea + Patch[j].inert_c[layer] * Patch[j].RelativeArea) / NewPatchArea;
			Patch[k].inert_n[layer] = (Patch[k].inert_n[layer] * Patch[k].RelativeArea + Patch[j].inert_n[layer] * Patch[j].RelativeArea) / NewPatchArea;
		}
		Patch[k].RelativeArea = NewPatchArea;
	}

	/// <summary>
	/// Controls the addition of several variables to the especified patches
	/// </summary>
	/// <param name="PatchesToAdd">The list of patches to which the stuff will be added</param>
	/// <param name="StuffToAdd">The values of the variables to add (supplied as deltas)</param>
	private void AddStuffToPatches(List<int> PatchesToAdd, AddSoilCNPatchwithFOMType StuffToAdd)
	{
		// Relevant data passed from OnAddSoilCNPatch event - these are all considered deltas:
		//.Urea: amount of urea to add per layer (kgN/ha)
		//.NH4: amount of ammonium to add per layer (kgN/ha)
		//.NO3: amount of nitrate to add per layer (kgN/ha)
		//.FOM: fresh organic matter to add, per fom pool
		//   .type: type of the given pool being altered (not used here yet)
		//   .Pool[]: info about FOM pools being added
		//      .C: amount of carbon in given pool to add per layer (kgC/ha)
		//      .N: amount of nitrogen in given pool to add per layer (kgN/ha)

		for (int i = PatchesToAdd.Count - 1; i >= 0; i--)
		{
			if ((StuffToAdd.Urea != null) && SumDoubleArray(StuffToAdd.Urea) > epsilon)
				Patch[PatchesToAdd[i]].dlt_urea = StuffToAdd.Urea;
			if ((StuffToAdd.NH4 != null) && SumDoubleArray(StuffToAdd.NH4) > epsilon)
				Patch[PatchesToAdd[i]].dlt_nh4 = StuffToAdd.NH4;
			if ((StuffToAdd.NO3 != null) && SumDoubleArray(StuffToAdd.NO3) > epsilon)
				Patch[PatchesToAdd[i]].dlt_no3 = StuffToAdd.NO3;
			if ((StuffToAdd.FOM != null) && (StuffToAdd.FOM.Pool != null))
			{
				bool SomethingAdded = false;
				double[][] CValues = new double[3][];
				double[][] NValues = new double[3][];
				for (int pool = 0; pool < StuffToAdd.FOM.Pool.Length; pool++)
				{
					if ((StuffToAdd.FOM.Pool[pool].C != null) && (SumDoubleArray(StuffToAdd.FOM.Pool[pool].C) > epsilon))
					{
						CValues[pool] = StuffToAdd.FOM.Pool[pool].C;
						SomethingAdded = true;
					}
					if ((StuffToAdd.FOM.Pool[pool].N != null) && (SumDoubleArray(StuffToAdd.FOM.Pool[pool].N) > epsilon))
					{
						NValues[pool] = StuffToAdd.FOM.Pool[pool].N;
						SomethingAdded = true;
					}
				}
				if (SomethingAdded)
				{
					Patch[PatchesToAdd[i]].dlt_fom_c = CValues;
					Patch[PatchesToAdd[i]].dlt_fom_n = NValues;
				}
			}
		}
	}

	/// <summary>
	/// Checks whether two patches can be considered equal
	/// </summary>
	/// <param name="k">Patch used as reference</param>
	/// <param name="j">Patch being compared to reference</param>
	/// <returns>TRUE if patches are similar enough, FALSE otherwise</returns>
	private bool PatchesAreEqual(int k, int j)
	{
		double deltaValue = 0.0;
		double TotalValueBase = 0.0;
		int TestedCount = 0;
		int AgreedCount = 0;
		bool Result = false;

		double AdjustFactor = (k == 0 ? 1.0 : DiffAdjustFactor);   // an adjust factor is applied, if patch is base the diffs can be a bit less stringent

		// go through a series of criteria to evaluate whether the two patches can be considered equal

		// test Total C
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].carbon_tot) - SumDoubleArray(Patch[j].carbon_tot));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].carbon_tot));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalOrgC, absoluteDiff_TotalOrgC, AdjustFactor);

		// test Total N
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].nit_tot) - SumDoubleArray(Patch[j].nit_tot));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].nit_tot));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalOrgN, absoluteDiff_TotalOrgN, AdjustFactor);

		// test M. Biomass C
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].biom_c) - SumDoubleArray(Patch[j].biom_c));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].biom_c));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalBiomC, absoluteDiff_TotalBiomC, AdjustFactor);

		// test Total Urea
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].urea) - SumDoubleArray(Patch[j].urea));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].urea));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalUrea, absoluteDiff_TotalUrea, AdjustFactor);

		// test Total NH4
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].nh4) - SumDoubleArray(Patch[j].nh4));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].nh4));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalNH4, absoluteDiff_TotalNH4, AdjustFactor);

		// test Total NO3
		deltaValue = Math.Abs(SumDoubleArray(Patch[k].no3) - SumDoubleArray(Patch[j].no3));
		TotalValueBase = Math.Abs(SumDoubleArray(Patch[k].no3));
		TestedCount += 1;
		AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_TotalNO3, absoluteDiff_TotalNO3, AdjustFactor);

		// tests by layer (from surface down to a give depth to test) - these are tested im ppm, not kg/ha
		for (int layer = 0; layer <= LayerDepthToTestDiffs; layer++)
		{
			// test M. biomass
			deltaValue = MathUtility.Divide(Math.Abs(Patch[k].biom_c[layer] - Patch[j].biom_c[layer]), convFactor[layer], 0.0);
			TotalValueBase = MathUtility.Divide(Patch[k].biom_c[layer], convFactor[layer], 0.0);
			TestedCount += 1; 
			AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_LayerBiomC, absoluteDiff_LayerBiomC, AdjustFactor);

			// test urea
			deltaValue = MathUtility.Divide(Math.Abs(Patch[k].urea[layer] - Patch[j].urea[layer]), convFactor[layer], 0.0);
			TotalValueBase = MathUtility.Divide(Patch[k].urea[layer], convFactor[layer], 0.0);
			TestedCount += 1;
			AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_LayerUrea, absoluteDiff_LayerUrea, AdjustFactor);

			// test NH4
			deltaValue = MathUtility.Divide(Math.Abs(Patch[k].nh4[layer] - Patch[j].nh4[layer]), convFactor[layer], 0.0);
			TotalValueBase = MathUtility.Divide(Patch[k].nh4[layer], convFactor[layer], 0.0);
			TestedCount += 1;
			AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_LayerNH4, absoluteDiff_LayerNH4, AdjustFactor);

			// test NO3
			deltaValue = MathUtility.Divide(Math.Abs(Patch[k].no3[layer] - Patch[j].no3[layer]), convFactor[layer], 0.0);
			TotalValueBase = MathUtility.Divide(Patch[k].no3[layer], convFactor[layer], 0.0);
			TestedCount += 1;
			AgreedCount += TestDelta(deltaValue, TotalValueBase, relativeDiff_LayerNO3, absoluteDiff_LayerNO3, AdjustFactor);
		}

		Result = ((AgreedCount - TestedCount) == 0);
		return Result;
	}

	private int TestDelta(double deltaValue, double TotalValueBase, double relativeDiffFactor, double absoluteDiffFactor, double AdjustFactor)
	{
		int Result = 0;
		if (deltaValue <= absoluteDiffFactor * AdjustFactor)
			Result = 1;
		if ((TotalValueBase > 0) && (Result < 1))
			if (deltaValue / TotalValueBase <= relativeDiffFactor * AdjustFactor)
				Result = 1;

		return Result;
	}

	/// <summary>
	/// Check the list of patch names and IDs passed by 'AddSoilCNPatch' event
	/// </summary>
	/// <remarks>
	/// Tasks performed by this method:
	///  - Verify whether there are replicates in the list given
	///  - Verify whether the IDs and/or names given correspond to existing patches
	///  - Eliminate replicates and consolidate lists of IDs and names (merge both)
	/// </remarks>
	/// <param name="IDsToCheck">List of IDs or indices of patches</param>
	/// <param name="NamesToCheck">List of names of patches</param>
	/// <returns>List of patch IDs (negative if no ID is found)</returns>
	private List<int> CheckPatchIDs(int[] IDsToCheck, string[] NamesToCheck)
	{
		// List of patch IDs for output
		List<int> SelectedIDs = new List<int>();

		// 1. Check names
		if (NamesToCheck.Length > 0)
		{  // at least one name was given, check for existence and get ID
			for (int i_name = 0; i_name < NamesToCheck.Length; i_name++)
			{
				bool isReplicate = false;
				// check for replicates
				for (int i = 0; i < i_name; i++)
					if (NamesToCheck[i] == NamesToCheck[i_name])
						isReplicate = true;

				if (!isReplicate)
				{
					// Check for patch existence
					for (int k = 0; k < Patch.Count; k++)
					{
						if (NamesToCheck[i_name] == Patch[k].PatchName)
						{
							// found the patch, add to list
							SelectedIDs.Add(k);
							k = Patch.Count;
						}
						// else{}  continue looking for next name
					}
				}
			}
		}
		// else{} No names were given

		// 1. Check IDs
		if (IDsToCheck.Length > 0)
		{  // at least one ID was given, check for existence and get ID
			for (int i_id = 0; i_id < IDsToCheck.Length; i_id++)
			{
				bool isReplicate = false;
				if (SelectedIDs.Count > 0)
				{ // there are IDs in the list already, check for replicates
					for (int i = 0; i < SelectedIDs.Count; i++)
						if (SelectedIDs[i] == IDsToCheck[i_id])
						{ // already selected
							isReplicate = true;
							i = SelectedIDs.Count;
						}
				}
				// check for replicates in list given
				for (int i = 0; i < i_id; i++)
					if (IDsToCheck[i] == IDsToCheck[i_id])
						isReplicate = true;
				if (!isReplicate)

					// Check for patch existence
					for (int k = 0; k < Patch.Count; k++)
					{
						if (IDsToCheck[i_id] == k)
						{
							// found the patch, add to list
							SelectedIDs.Add(k);
							k = Patch.Count;
						}
						// else{}  continue looking for next name
					}
			}
		}
		// else{} No IDs were given

		if (SelectedIDs.Count == 0)
		{ // no valid patch was found, notify user
			string myMessage = " No valid patch was found to base the new patch being added - operation will be ignored";
			writeMessage(myMessage);
		}
		return SelectedIDs;
	}

	/// <summary>
	/// calculate how the dlt's (C and N) are partitioned amongst patches
	/// </summary>
	/// <param name="incomingDelta">The dlt to be partioned amongst patches</param>
	/// <param name="SoluteName">The solute or pool that is changing</param>
	/// <returns>The values of dlt partitioned for each existing patch</returns>
	private double[][] partitionDelta(double[] incomingDelta, string SoluteName, string PartitionType)
	{
		int nLayers = dlayer.Length;

		// 1. initialise the result array
		double[][] Result = new double[Patch.Count][];
		for (int k = 0; k < Patch.Count; k++)
			Result[k] = new double[nLayers];

		try
		{
			// 2- gather how much solute is already in the soil
			double[][] alreadyThere = new double[Patch.Count][];
			for (int k = 0; k < Patch.Count; k++)
			{
				switch (SoluteName)
				{
					case "Urea":
						alreadyThere[k] = Patch[k].urea;
						break;
					case "NH4":
						alreadyThere[k] = Patch[k].nh4;
						break;
					case "NO3":
						alreadyThere[k] = Patch[k].no3;
						break;
					default:
						throw new Exception(" The solute " + SoluteName
							+ " is not recognised by SoilNitrogen -  solute partition");
				}
			}

			// 3- calculations are done for each layer 
			for (int layer = 0; layer < (nLayers); layer++)
			{
				double totalSolute = 0.0;
				double[] patchSolute = new double[Patch.Count];

				// these are only needed if using BasedOnSoilConcentration and delta is negative -----------------------------------------
				//   (need to veryfy that the N removed will not result in negative content)
				double totalSoluteLr = 0.0;
				double[] patchSoluteLr = new double[Patch.Count];
				// -----------------------------------------------------------------------------------------------------------------------

				// 3.1- get the solute amounts, total and each patch
				if ((PartitionType == "BasedOnLayerConcentration".ToLower()) ||
					(PartitionType == "BasedOnConcentrationAndDelta".ToLower() & incomingDelta[layer] <= 0.0))
				{
					for (int k = 0; k < Patch.Count; k++)
						patchSolute[k] = alreadyThere[k][layer]* Patch[k].RelativeArea;
					totalSolute = SumDoubleArray(patchSolute);
				}
				else if ((PartitionType == "BasedOnSoilConcentration".ToLower()) ||
						 (PartitionType == "BasedOnConcentrationAndDelta".ToLower() & incomingDelta[layer] > 0.0))
				{
					for (int k = 0; k < Patch.Count; k++)
					{
						double layerUsed = 0.0;
						for (int z = layer; z >= 0; z--)		// goes backwards till soil surface
						{
							patchSolute[k] += alreadyThere[k][z] * Patch[k].RelativeArea;
							layerUsed += dlayer[z];
							if ((LayerNPartition > epsilon) && (layerUsed >= LayerNPartition))	// stop if thickness reaches a defined value
								z = 0;
						}
						totalSolute += patchSolute[k] * Patch[k].RelativeArea;

						// this is only needed when using BasedOnSoilConcentration and delta is negative   -------------------------------
						if ((incomingDelta[layer] < 0.0) && (PartitionType == "BasedOnSoilConcentration".ToLower()))
							patchSoluteLr[k] = alreadyThere[k][layer] * Patch[k].RelativeArea;
						// ----------------------------------------------------------------------------------------------------------------

					}
					totalSolute = SumDoubleArray(patchSolute);
					totalSoluteLr = SumDoubleArray(patchSoluteLr);	// only needed when using BasedOnSoilConcentration and delta is negative   ----
				}

				// 3.2 - Compute the partition weights for each patch
				
				double[] weight = new double[Patch.Count];
				for (int k = 0; k < Patch.Count; k++)
				{ // weights for the nomimal NPartitionApproach
					weight[k] = 1.0;
					if (totalSolute > 0)
						weight[k] = MathUtility.Divide(patchSolute[k], totalSolute, 0.0);
				}

				// this is only needed when using BasedOnSoilConcentration and delta is negative -----------------------------------------
				bool test = true;
				if (PartitionType == "BasedOnSoilConcentration".ToLower() && test)
				{
					//3.2.1. partial weights
					double[] weight1 = new double[Patch.Count];
					double[] weight2 = new double[Patch.Count];
					for (int k = 0; k < Patch.Count; k++)
					{
						// weights for the nomimal NPartitionApproach
						weight1[k] = 1.0;
						if (totalSolute > 0)
							weight1[k] = MathUtility.Divide(patchSolute[k], totalSolute, 0.0);
						// weights for LayerConcentration approach
						weight2[k] = 1.0;
						if (totalSoluteLr > 0)
							weight2[k] = MathUtility.Divide(patchSoluteLr[k], totalSoluteLr, 0.0);
					}

					// 3.2.2. get the minimun of the weights
					double TotalWeight1 = weight1.Sum();
					double TotalWeight2 = weight2.Sum();
					for (int k = 0; k < Patch.Count; k++)
						weight[k] = Math.Min(weight1[k], weight2[k]);

					// 3.2.3. finally calculate the actual weights
					double TotalWeight = weight.Sum();
					for (int k = 0; k < Patch.Count; k++)
					{
						if (TotalWeight > 0)

							weight[k] = MathUtility.Divide(weight[k], TotalWeight, 0.0);
						else
							weight[k] = 1.0;
					}
				}    // ------------------------------------------------------------------------------------------------------------------


				// 3.3- Compute the partitioned  values for each patch
				for (int k = 0; k < Patch.Count; k++)
					Result[k][layer] = (incomingDelta[layer] * weight[k]) / Patch[k].RelativeArea;

			}
		}
		catch (Exception e)
		{
			throw new Exception(" problems with partitioning " + SoluteName + "- " + e.ToString());
		}

		return Result;
	}

}
